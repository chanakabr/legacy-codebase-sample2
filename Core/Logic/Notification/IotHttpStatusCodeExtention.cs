using Phx.Lib.Log;
using System.Collections.Generic;
using System.Net;

namespace ApiLogic.Notification
{
    internal static class IotHttpStatusCodeExtention
    {
        private static readonly KLogger Logger = new KLogger(nameof(IotManager));
        static readonly HashSet<HttpStatusCode> SupportedStatusesFromAdapter = new HashSet<HttpStatusCode>
        {
            HttpStatusCode.NoContent, HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.PartialContent
        };

        internal static bool IsServiceStatusDefined(this HttpStatusCode httpStatusCode)
        {
            //aws rate limit specific error
            if (httpStatusCode.Equals(HttpStatusCode.PartialContent))
            {
                Logger.Warn($"Recieved a rate limit exception");
            }
            return SupportedStatusesFromAdapter.Contains(httpStatusCode);
        }
    }
}