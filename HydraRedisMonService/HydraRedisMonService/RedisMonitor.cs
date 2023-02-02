using Hydra4NET;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HydraRedisMonService
{
    public class RedisMonitor
    {
        private const int _ONE_SECOND = 1;
        private IHydra _hydra;
        private ILogger<RedisMonitor> _logger;
        private IConnectionMultiplexer? _redis;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private int _secondsTick = 1;
        private const string _redis_pre_key = "hydra:service";
        private const string _nodes_hash_key = "hydra:service:nodes";

        public RedisMonitor(IHydra hydra, ILogger<RedisMonitor> logger)
        {
            _hydra = hydra;
            _logger = logger;
            _redis = _hydra.GetRedisConnection();
        }

        private async Task HeartBeat()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    //await PresenceEvent();
                    await Task.Delay(_ONE_SECOND * 1000, _cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void Dispose()
        {
            try
            {
                _cts.Cancel();
            }
            finally
            {
                _cts.Dispose();
            }
        }
    }
}
