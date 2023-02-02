using Hydra4NET;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;

namespace HydraRedisMonService
{
    internal class CoreService : BackgroundService
    {
        private IHydra _hydra;
        private ILogger<CoreService> _logger;
        private IConnectionMultiplexer? _redis;
        private int _secondsTick = 0;
        private const string _redis_pre_key = "hydra:service";
        private const string _nodes_hash_key = "hydra:service:nodes";
        private const int _ONE_SECOND = 1000;
        private const int _sweepIntervalInSeconds = 60;

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
                    await Task.Delay(_ONE_SECOND);
                    _secondsTick = _secondsTick + 1;
                    if (_secondsTick > _sweepIntervalInSeconds)
                    {
                        DateTime datetime = DateTime.Now;
                        _logger.LogInformation($"HeartBeat: {datetime.ToUniversalTime().ToString("u").Replace(" ", "T")}");
                        _secondsTick = 0;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ConnectionService failed");
            }
        }
    }
}
