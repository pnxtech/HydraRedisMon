using Hydra4Net.HostingExtensions;
using Hydra4NET;
using Microsoft.Extensions.Logging;

namespace HydraRedisMonService
{
    internal class ServiceMessageHandler : HydraEventsHandler
    {
        private ILogger<ServiceMessageHandler> _logger;

        public ServiceMessageHandler(ILogger<ServiceMessageHandler> logger, HydraConfigObject config)
        {
            _logger = logger;
            //add to appsettings.json file or env
            SetValidateMode(config);
        }

        void SetValidateMode(HydraConfigObject config)
        {
        }

        public override async Task OnMessageReceived(IInboundMessage msg, IHydra hydra)
        {
            try
            {
                await Task.Delay(0);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "OnMessageReceived failed");
            }
        }

        public override async Task OnQueueMessageReceived(IInboundMessage msg, IHydra hydra)
        {
            await Task.Delay(0);
        }

        private async Task HandleQueuerType(IInboundMessage msg, IHydra hydra)
        {
            await Task.Delay(0);
        }

        private async Task HandleRespondType(IInboundMessage msg, IHydra hydra)
        {
            await Task.Delay(0);
        }

        private async Task HandleResponseStreamType(IInboundMessage msg, IHydra hydra)
        {
            await Task.Delay(0);
        }

        #region Optional
        public override Task BeforeInit(IHydra hydra)
        {
            _logger.LogInformation($"Hydra initialized");
            return base.BeforeInit(hydra);
        }

        public override Task OnShutdown(IHydra hydra)
        {
            _logger.LogInformation($"Hydra shut down");
            return base.OnShutdown(hydra);
        }

        public override Task OnInitError(IHydra hydra, Exception e)
        {
            _logger.LogCritical(e, "A fatal error occurred initializing Hydra");
            return base.OnInitError(hydra, e);
        }

        public override Task OnDequeueError(IHydra hydra, Exception e)
        {
            _logger.LogWarning(e, "An error occurred while dequeueing Hydra");
            return base.OnDequeueError(hydra, e);
        }

        #endregion Optional
    }
}
