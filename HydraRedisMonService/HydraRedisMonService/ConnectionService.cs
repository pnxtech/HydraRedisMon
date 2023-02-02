using Hydra4NET;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HydraRedisMonService
{
    internal class ConnectionService : BackgroundService
    {
        private IHydra _hydra;
        private ILogger<ConnectionService> _logger;

        public ConnectionService(IHydra hydra, ILogger<ConnectionService> logger)
        {
            _hydra = hydra;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                // since this code may be called before Hydra is finished initializing, GetRedisConnectionAsync()
                // will ensure that initialization is complete before returning the connection.
                var redis = await _hydra.GetRedisConnectionAsync();
                _logger.LogInformation($"Redis client name: {redis.ClientName}");
                redis = await _hydra.GetRedisConnectionAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ConnectionService failed");
            }
        }
    }
}
