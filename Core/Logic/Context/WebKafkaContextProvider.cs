using System;
using System.Threading;
using KalturaRequestContext;
using OTT.Lib.Kafka;

namespace ApiLogic.Context
{
    public class WebKafkaContextProvider : IKafkaContextProvider
    {
        private static readonly Lazy<WebKafkaContextProvider> Lazy = new Lazy<WebKafkaContextProvider>(
            () => new WebKafkaContextProvider(RequestContextUtilsInstance.Get()),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IRequestContextUtils _requestContextUtils;

        public static WebKafkaContextProvider Instance => Lazy.Value;

        public WebKafkaContextProvider(IRequestContextUtils requestContextUtils)
        {
            _requestContextUtils = requestContextUtils;
        }

        public string GetRequestId()
        {
            return _requestContextUtils.GetRequestId();
        }

        public long? GetPartnerId()
        {
            return _requestContextUtils.GetPartnerId();
        }

        public long? GetUserId()
        {
            return _requestContextUtils.GetUserId();
        }
    }
}