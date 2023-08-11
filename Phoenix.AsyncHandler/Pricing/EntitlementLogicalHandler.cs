using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using OTT.Lib.Kafka.Extensions;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Logical.appstoreNotification;

namespace Phoenix.AsyncHandler.Pricing
{
    public class EntitlementLogicalHandler : IKafkaMessageHandler<AppstoreNotification>
    {
        private readonly ILogger<EntitlementLogicalHandler> _logger;

        public EntitlementLogicalHandler(ILogger<EntitlementLogicalHandler> logger)
        {
            _logger = logger;
        }
        
        public Task<HandleResult> Handle(ConsumeResult<string, AppstoreNotification> consumeResult)
        {
            switch (consumeResult.Result.Message.Value.State)
            {
                case State.SubscriptionCanceled: return Task.FromResult(CancelSubscriptionRenewal(consumeResult));
                default: return Task.FromResult(Result.Ok);
            }
        }
        
        protected HandleResult CancelSubscriptionRenewal(ConsumeResult<string, AppstoreNotification> consumeResult)
        {
            var appstoreNotification = consumeResult.GetValue(); 
            var orderId = appstoreNotification.ExternalTransactionId;
            var source = appstoreNotification.NotificationSource.ToString();
            if (appstoreNotification.PartnerId.HasValue)
            {
                var status =  Core.ConditionalAccess.Module.CancelSubscriptionRenewalAfterAppStoreEvent((int)appstoreNotification.PartnerId.Value, source, orderId);
                if (!status.IsOkStatusCode())
                {
                    _logger.LogError(status.Message);
                }
            } else {
                _logger.LogError("PartnerId not exists");
            }
            return Result.Ok;
        }
    }
}
