using Hydra4NET;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace HydraRedisMonService
{
    internal class CoreService : BackgroundService
    {
        private IHydra _hydra;
        private IOptions<AppSettings> _config;
        private ILogger<CoreService> _logger;
        private IConnectionMultiplexer? _redis;
        private int _secondsTick = 0;
        private const string _nodes_hash_key = "hydra:service:nodes";
        private const int _ONE_SECOND = 1000;
        private int _sweepIntervalInSeconds = 60;
        private int _redisHydraDb = 0;

        public CoreService(IHydra hydra, IOptions<AppSettings> config, ILogger<CoreService> logger)
        {
            _hydra = hydra;
            _config = config;
            _logger = logger;
            _sweepIntervalInSeconds = _config.Value.SweepIntervalInSeconds;
            _redisHydraDb = _config.Value.RedisHydraDb;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _redis = await _hydra.GetRedisConnectionAsync();
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (_secondsTick++ > _sweepIntervalInSeconds)
                    {
                        var db = _redis?.GetDatabase(_redisHydraDb);
                        if (db != null)
                        {
                            PresenceNodeEntryCollection nodes = await _hydra.GetServiceNodesAsync();
                            foreach (var entry in nodes)
                            {
                                if (entry.Elapsed > _sweepIntervalInSeconds)
                                {
                                    _logger.LogInformation($"Clearing service entry for {entry.ServiceName} - Hostname:{entry.HostName} InstanceID: {entry.InstanceID}");
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
