using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using KLogMonitor;

namespace CDNTokenizers.Tokenizers
{
    public class ConcurrentCDNTokenizer : BaseCDNTokenizer
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected static readonly string ALPHA_NUMERIC_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        protected static readonly Random RANDOM_GEN = new Random();
        protected static readonly string CONCURRENT_EXPIRATION_FORMAT = Utils.GetConfigValue("concurrent_expiration_format");
        protected static readonly string CDNTOKEN_GROUPS_WITH_IPS = Utils.GetConfigValue("CdnTokenGroupsWithIps");
        protected byte[] m_sSaltBytes;

        public ConcurrentCDNTokenizer(int nGroupID, int nStreamingCompanyID)
            : base(nGroupID, nStreamingCompanyID)
        {

        }

        internal override void Init()
        {
            base.Init();
            m_sSaltBytes = System.Text.Encoding.ASCII.GetBytes(m_sSalt);

        }

        public override string GenerateToken(Dictionary<string, string> dParams)
        {
            string resultURL = string.Empty;
            try
            {
                #region get query params
                string url = GetUrl(dParams);
                string assetname = GetAssetName(url);
                DateTime expiration = GetExpirationTime();
                string sessionID = GenerateSessionID();

                // Check if group exists in the CdnTokenGroupsWithIps tcm configuration
                bool groupWithIp = false;
                if (!string.IsNullOrEmpty(CDNTOKEN_GROUPS_WITH_IPS))
                {
                    groupWithIp = CDNTOKEN_GROUPS_WITH_IPS.Split(';').Any(p => p.Trim() == m_nGroupID.ToString());
                }

                string ip = string.Empty;
                if (groupWithIp)
                {
                    ip = GetIP(dParams);
                }

                string format = string.IsNullOrEmpty(CONCURRENT_EXPIRATION_FORMAT) ? "yyyy-MM-ddTHH:mm:ssZ" : CONCURRENT_EXPIRATION_FORMAT;
                string expirationStr = expiration.ToString(format).ToLower();

                string strToSign = string.Format("{0};{1};{2};{3}", assetname, ip, sessionID, expirationStr);

                string hashStr = SignString(strToSign);

                //create query string as specified in spec.
                string queryStr = string.Format("c={0}&s={1}&e={2}&t={3}", ip, sessionID, expirationStr, hashStr);
                #endregion


                #region build uri with query
                UriBuilder baseUri = new UriBuilder(url);
                Utils.AddQueryStringParams(ref baseUri, queryStr);

                var splitUrl = baseUri.Uri.ToString().Split('?');

                if (splitUrl.Length > 1)
                {
                    resultURL = string.Format("{0}?{1}", splitUrl[0], splitUrl[1].Replace(":", "%3A"));
                }
                else
                {
                    resultURL = splitUrl[0];
                }

                #endregion

            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Concurrent CDN - caught exception when generating token. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
            }


            return resultURL;
        }

        protected string GetAssetName(string sUrl)
        {
            string assetname = string.Empty;
            try
            {
                Uri uri = new Uri(sUrl);
                assetname = uri.Segments[uri.Segments.Length - 1];
            }
            catch
            {
            }

            return assetname;
        }

        protected string GetIP(Dictionary<string, string> dParams)
        {
            return dParams.ContainsKey(Constants.IP) ? dParams[Constants.IP] : string.Empty;
        }

        protected DateTime GetExpirationTime()
        {
            return DateTime.UtcNow.AddSeconds(m_nTTL);
        }

        protected string GenerateSessionID()
        {
            string result = RANDOM_GEN.Next(10000000, 99999999).ToString();
            return result;
        }

        protected string SignString(string message)
        {
            byte[] hashBytes = Utils.SignHmacSha1(m_sSaltBytes, System.Text.Encoding.UTF8.GetBytes(message));
            string hashStr = Utils.HexStringFromBytes(hashBytes);

            return hashStr;
        }
    }
}
