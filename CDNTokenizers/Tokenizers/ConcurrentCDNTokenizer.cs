using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CDNTokenizers.Tokenizers
{
    public class ConcurrentCDNTokenizer : BaseCDNTokenizer
    {

        protected static readonly string ALPHA_NUMERIC_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        protected static readonly Random RANDOM_GEN = new Random();

        protected byte[] m_sSaltBytes;

        public ConcurrentCDNTokenizer(int nGroupID, int nStreamingCompanyID)
            : base(nGroupID, nStreamingCompanyID)
        {

        }

        public override void Init()
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
                string ip = dParams["ip"];
                DateTime expiration = GetExpirationTime();
                string sessionID = GenerateSessionID();

                string strToSign = string.Format("{0};{1};{2};{3}", assetname, ip, sessionID, expiration.ToString());

                string hashStr = SignString(strToSign);

                //create query string as specified in spec.
                string queryStr = string.Format("c={0}&s={1}&e={2}&t={3}", ip, sessionID, expiration, hashStr);
                #endregion


                #region build uri with query
                UriBuilder baseUri = new UriBuilder(url);

                if (baseUri.Query != null && baseUri.Query.Length > 1)
                    baseUri.Query = string.Concat(baseUri.Query.Substring(1), "&", queryStr);
                else
                    baseUri.Query = queryStr;

                resultURL = baseUri.Uri.ToString();
                #endregion

            }
            catch (Exception ex)
            {
            }


            return resultURL;
        }

        protected string GetAssetName(string sUrl)
        {
            string assetname = string.Empty;

            if (Uri.CheckSchemeName(sUrl))
            {
                Uri uri = new Uri(sUrl);
                assetname = uri.Segments[uri.Segments.Length - 1];
            }

            return assetname;
        }

        protected string GetIP(Dictionary<string, string> dParams)
        {
            return dParams.ContainsKey("ip") ? dParams["ip"] : string.Empty;
        }

        protected DateTime GetExpirationTime()
        {
            return DateTime.UtcNow.AddSeconds(m_nTTL);
        }

        protected string GenerateSessionID()
        {
            string result = new string(Enumerable.Repeat(ALPHA_NUMERIC_CHARS, 8).Select(s => s[RANDOM_GEN.Next(s.Length)]).ToArray());

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
