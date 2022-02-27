using System;
using OTT.Lib.Kafka;
using Phx.Lib.Log;

namespace IngestHandler.Common.Kafka
{
    public class IngestKafkaContextProvider : IKafkaContextProvider
    {
        public string GetRequestId()
        {
            var requestId = KLogger.LogContextData[Constants.REQUEST_ID_KEY]?.ToString();

            return requestId;
        }

        public long? GetPartnerId()
        {
            var partnerIdObject = KLogger.LogContextData[Constants.GROUP_ID];
            var partnerId = partnerIdObject == null
                ? (long?)null
                : Convert.ToInt64(partnerIdObject);

            return partnerId;
        }

        public long? GetUserId()
        {
            var userIdObject = KLogger.LogContextData[Constants.USER_ID];
            var userId = userIdObject == null
                ? (long?)null
                : Convert.ToInt64(userIdObject);

            return userId;
        }
    }
}