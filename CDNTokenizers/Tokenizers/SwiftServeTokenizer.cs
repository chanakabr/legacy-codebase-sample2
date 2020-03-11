using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace CDNTokenizers.Tokenizers
{
    public class SwiftServeTokenizer : BaseCDNTokenizer
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected byte[] m_sSaltBytes;

        public SwiftServeTokenizer(int nGroupID, int nStreamingCompanyID)
            : base(nGroupID, nStreamingCompanyID)
        {

        }

        internal override void Init()
        {
            base.Init();
            m_sSaltBytes = System.Text.Encoding.UTF8.GetBytes(m_sSalt);
        }

        public override string GenerateToken(Dictionary<string, string> dParams)
        {
            string resultURL = string.Empty;

            try
            {
                string url = GetUrl(dParams);
                UriBuilder uriBuilder = new UriBuilder(url);

                string ip = GetIP(dParams);
                string startTime = GetStartTime();
                string endTime = GetEndTime();

                string queryParams = string.Format("stime={0}&etime={1}&ip={2}", startTime, endTime, ip);
                Utils.AddQueryStringParams(ref uriBuilder, queryParams);

                string hashStr = SignString(uriBuilder.Uri.PathAndQuery);
                Utils.AddQueryStringParams(ref uriBuilder, string.Concat("encoded=", hashStr));

                resultURL = uriBuilder.Uri.ToString();
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("SwiftServeTokenizer - caught exception while generating token. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return resultURL;
        }

        protected string GetStartTime()
        {
            return DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        }

        protected string GetEndTime()
        {
            return DateTime.UtcNow.AddSeconds(m_nTTL).ToString("yyyyMMddHHmmss");
        }

        protected string GetIP(Dictionary<string, string> dParams)
        {
            return dParams.ContainsKey(Constants.IP) ? dParams[Constants.IP] : string.Empty;
        }

        protected string SignString(string message)
        {
            byte[] hashBytes = Utils.SignHmacSha1(m_sSaltBytes, System.Text.Encoding.UTF8.GetBytes(message));
            string hashStr = Utils.HexStringFromBytes(hashBytes);

            string result = (hashStr.Length > 20) ? string.Concat("0", hashStr.Substring(0, 20)) : string.Concat("0", hashStr);

            return result;
        }
    }
}
