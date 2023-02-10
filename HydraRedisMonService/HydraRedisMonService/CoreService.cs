using Hydra4NET;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Collections.Generic;

namespace HydraRedisMonService
{
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
                            PresenceNodeEntryCollection nodes = await _hydra.GetServiceNodesAsync();
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
    }
}
