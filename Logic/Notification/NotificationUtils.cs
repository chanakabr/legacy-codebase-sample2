using Core.Catalog.Request;
using Core.Catalog.Response;
using KLogMonitor;
using System;
using System.Reflection;
using System.Security.Cryptography;

namespace Core.Notification
{
    public class NotificationUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static string CATALOG_WS = "WS_Catalog";

        public static string GetSignature(string signString, string signatureKey)
        {
            string retVal;
            //Get key from DB
            string hmacSecret = signatureKey;
            // The HMAC secret as configured in the skin
            // Values are always transferred using UTF-8 encoding
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            // Calculate the HMAC
            // signingString is the SignString from the request
            HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret));
            retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signString)));
            myhmacsha1.Clear();
            return retVal;
        }

        public static UnifiedSearchResponse GetUnifiedSearchResponse(BaseRequest request)
        {
            BaseResponse response = null;

            try
            {
                response = request.GetResponse(request);
            }
            catch (Exception ex)
            {
                log.Error("GetUnifiedSearchResponse: error when calling catalog: ", ex);
            }

            return response as UnifiedSearchResponse;
        }
    }
}
