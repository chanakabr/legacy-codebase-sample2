using OTT.Lib.Kafka;

namespace Phoenix.AsyncHandler.Kafka
{
    internal sealed class AsyncHandlerKafkaContextProvider : IKafkaContextProvider
    {
        private string _requestId;
        private long? _partnerId;
        private long? _userId;
        
        public string GetRequestId()
        {
            return _requestId;
        }

        public long? GetPartnerId()
        {
            return _partnerId;
        }

        public long? GetUserId()
        {
            return _userId;
        }

        internal void Populate(string requestId, long? partnerId, long? userId)
        {
            _requestId = requestId;
            _partnerId = partnerId;
            _userId = userId;
        }
    }
}
