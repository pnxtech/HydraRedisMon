using Hydra4NET;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using static Hydra4NET.Hydra;
using System.Text.Json;
using System.Collections.Generic;

namespace HydraRedisMonService
{
    public class PresenceNodeEntry2
    {
        public string? ServiceName { get; set; }
        public string? ServiceDescription { get; set; }
        public string? Version { get; set; }
        public string? InstanceID { get; set; }
        public int ProcessID { get; set; }
        public string? Ip { get; set; }
        public string? Port { get; set; }
        public string? HostName { get; set; }
        public string? UpdatedOn { get; set; }
        public int Elapsed { get; set; }
    }

    public class PresenceNodeEntryCollection2 : List<PresenceNodeEntry2>
    {
        public PresenceNodeEntryCollection2() : base() { }
    }

    internal class CoreService : BackgroundService
    {
        private IHydra _hydra;
        private ILogger<CoreService> _logger;
        private IConnectionMultiplexer? _redis;
        private int _secondsTick = 0;
        private const string _nodes_hash_key = "hydra:service:nodes";
        private const int _ONE_SECOND = 1000;
        private const int _sweepIntervalInSeconds = 1 * 60;

        public CoreService(IHydra hydra, ILogger<CoreService> logger)
        {
            _hydra = hydra;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _redis = await _hydra.GetRedisConnectionAsync();
                _logger.LogInformation($"Redis client name: {_redis.ClientName}");
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_secondsTick++ > _sweepIntervalInSeconds)
                    {
                        var db = _redis?.GetDatabase(0);
                        if (db != null)
                        {
                            PresenceNodeEntryCollection2 nodes = await GetServiceNodesAsync();
                            foreach (var entry in nodes)
                            {
                                if (entry.Elapsed > _sweepIntervalInSeconds)
                                {
                                    // non-async with FireAndForget to aid in pipelining
                                    // if an entry fails it will be retried during a future timer interval
                                    db.HashDelete(_nodes_hash_key, entry.InstanceID, flags: CommandFlags.FireAndForget);
                                }
                            }
                        }
                        _secondsTick = 0;
                    }
                    await Task.Delay(_ONE_SECOND);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ConnectionService failed");
            }
        }

        public async Task<PresenceNodeEntryCollection2> GetServiceNodesAsync()
        {
            var time1970 = new DateTime(1970, 1, 1);
            var timeNow = (int)(DateTime.Now.ToUniversalTime().Subtract(time1970)).TotalSeconds;
            PresenceNodeEntryCollection2 serviceEntries = new PresenceNodeEntryCollection2();
            var db = _redis?.GetDatabase(0);
            if (db != null)
            {
                HashEntry[] list = await db.HashGetAllAsync(_nodes_hash_key);
                foreach (var entry in list)
                {
                    PresenceNodeEntry2? presenceNodeEntry = JsonSerializer.Deserialize<PresenceNodeEntry2>(entry.Value, new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    if (presenceNodeEntry != null)
                    {
                        var date = DateTime.Parse(presenceNodeEntry.UpdatedOn, null, System.Globalization.DateTimeStyles.RoundtripKind);
                        var unixTimestamp = (int)(date.ToUniversalTime().Subtract(time1970)).TotalSeconds;
                        presenceNodeEntry.Elapsed = timeNow - unixTimestamp;
                        serviceEntries.Add(presenceNodeEntry);
                    }
                }
            }
            return serviceEntries;
        }
    }
}
