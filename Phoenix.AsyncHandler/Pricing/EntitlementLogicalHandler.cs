using System;
using System.Collections.Generic;
using Core.Users.Cache;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Logical.appstoreNotification;


namespace Phoenix.AsyncHandler.Pricing
{
    public class EntitlementLogicalHandler : Handler<AppstoreNotification>
    {

        private readonly ILogger<EntitlementLogicalHandler> _logger;

        public EntitlementLogicalHandler(IKafkaConsumerFactory consumerFactory, ILogger<EntitlementLogicalHandler> logger) :
            base(consumerFactory, "appstore-notification")
        {
            _logger = logger;
        }

        protected override string Topic() => AppstoreNotification.GetTopic();
        protected override HandleResult Handle(ConsumeResult<string, AppstoreNotification> consumeResult)
        {
            switch (consumeResult.Result.Message.Value.State)
            {
                case State.SubscriptionCanceled: return CancelSubscriptionRenewal(consumeResult);
                default: return Result.Ok;
            }
        }
        
        protected HandleResult CancelSubscriptionRenewal(ConsumeResult<string, AppstoreNotification> consumeResult)
        {
            var appstoreNotification = consumeResult.GetValue();
            var orderId = appstoreNotification.ExternalTransactionId;
            var source = appstoreNotification.NotificationSource.ToString();
            Core.ConditionalAccess.Module.CancelSubscriptionRenewalAfterAppStoreEvent(source, orderId);
            return Result.Ok;
        }
    }
}