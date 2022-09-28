using EventBus.Abstraction;
using IngestHandler.Common.Kafka;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phx.Lib.Log;

namespace IngestHandler.Common.Infrastructure
{
    public class KafkaConsumerInterceptor : IKafkaConsumerInterceptor
    {
        private readonly ILogger<KafkaConsumerInterceptor> _logger;
        private readonly ManualKafkaContextProvider _evtContext;
        private readonly ManualKafkaContextProvider _ctx;

        public KafkaConsumerInterceptor(IKafkaContextProvider kafkaCtx, IEventContext evtContext, ILogger<KafkaConsumerInterceptor> logger)
        {
            _ctx = kafkaCtx as ManualKafkaContextProvider;
            // we populate this as well for legacy componetns that relay on IEventContext from event bus like feature flags abstarction
            _evtContext = evtContext as ManualKafkaContextProvider;
            _logger = logger;
        }

        public void AfterMessageHandled(IKafkaContextProvider context, IConsumeResult consumeResult, HandleResult result)
        {
            _logger.LogDebug($"consumed message, key:[{consumeResult.Key}], topic:[{consumeResult.Topic}], partition:[{consumeResult.Partition}], offset:[{consumeResult.Offset}], partnerId:[{consumeResult.PartnerId}], result:[{result}]");
        }

        public void BeforeMessageHandled(IKafkaContextProvider context, IConsumeResult consumeResult)
        {
            KLogger.SetRequestId(context.GetRequestId());
            KLogger.SetGroupId(context.GetPartnerId()?.ToString());
            _ctx.PartnerId = context.GetPartnerId();
            _ctx.RequestId = context.GetRequestId();
            _ctx.UserId = context.GetUserId();

            _evtContext.PartnerId = context.GetPartnerId();
            _evtContext.RequestId = context.GetRequestId();
            _evtContext.UserId = context.GetUserId();

            _logger.LogDebug($"consuming new message, key:[{consumeResult.Key}], topic:[{consumeResult.Topic}], partition:[{consumeResult.Partition}], offset:[{consumeResult.Offset}], partnerId:[{consumeResult.PartnerId}]");
        }
    }
}