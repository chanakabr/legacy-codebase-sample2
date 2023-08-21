using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phx.Lib.Log;

namespace Phoenix.AsyncHandler.Kafka
{
    internal sealed class KafkaConsumerInterceptor : IKafkaConsumerInterceptor
    {
        private readonly IKafkaContextProvider _context;
        private readonly ILogger<KafkaConsumerInterceptor> _logger;

        public KafkaConsumerInterceptor(IKafkaContextProvider context, ILogger<KafkaConsumerInterceptor> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Before message handled event
        /// </summary>
        /// <param name="context">This parameter provides a <see cref="IConsumeResult"/> as a <see cref="IKafkaContextProvider"/></param>
        /// <param name="consumeResult"></param>
        public void BeforeMessageHandled(IKafkaContextProvider context, IConsumeResult consumeResult)
        {
            var ctx = _context as AsyncHandlerKafkaContextProvider;

            SetKLogger(context, consumeResult);
            PopulateContext(context, ctx);

            _logger.LogDebug($"consuming new message, key:[{consumeResult.Key}], topic:[{consumeResult.Topic}], partition:[{consumeResult.Partition}], offset:[{consumeResult.Offset}], partnerId:[{consumeResult.PartnerId}]");
        }

        public void AfterMessageHandled(IKafkaContextProvider context, IConsumeResult consumeResult, HandleResult result)
        {
            _logger.LogDebug($"consumed message, key:[{consumeResult.Key}], topic:[{consumeResult.Topic}], partition:[{consumeResult.Partition}], offset:[{consumeResult.Offset}], partnerId:[{consumeResult.PartnerId}], result:[{result}]");
        }

        private static void SetKLogger(IKafkaContextProvider context, IConsumeResult consumeResult)
        {
            KLogger.SetRequestId(context.GetRequestId());
            if (context.GetPartnerId().HasValue)
            {
                KLogger.SetGroupId(context.GetPartnerId().Value.ToString());
            }

            KLogger.SetTopic(consumeResult.Topic);
        }

        private static void PopulateContext(IKafkaContextProvider context, AsyncHandlerKafkaContextProvider ctx)
        {
            ctx?.Populate(context.GetRequestId(), context.GetPartnerId(), context.GetUserId());
        }
    }
}
