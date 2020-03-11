using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDNTokenizers.Tokenizers.AkamaiTokenizers
{
    public class AkamaiTokenizerTurner : AbstractAkamaiTokenizer
    {
        public AkamaiTokenizerTurner(int nGroupID, int nStreamingCompanyID)
            :base(nGroupID, nStreamingCompanyID)
        {
        }

        public override string GenerateToken(Dictionary<string, string> dParams)
        {
            string resultURL = string.Empty;
            try
            {
                AkamaiTokenConfig conf = InitAkamaiConfig(dParams);
                string token = GenerateToken(conf);

                string url = GetUrl(dParams);
                resultURL = (new Uri(url + "?hdnea=" + token, UriKind.Absolute)).ToString();
            }
            catch { }

            return resultURL;
        }

        /// <summary>
        /// Generates a token
        /// </summary>
        /// <param name="tokenConfig">Configuration values to create token</param>
        /// <returns></returns>
        private static string GenerateToken(AkamaiTokenConfig tokenConfig)
        {
            string mToken = tokenConfig.IPField + tokenConfig.StartTimeField + tokenConfig.ExpirationField + tokenConfig.AclField + tokenConfig.SessionIDField + tokenConfig.PayloadField;
            string digest = mToken + tokenConfig.UrlField + tokenConfig.SaltField;

            // calculate hmac
            string hmac = Utils.CalculateHMAC(digest.TrimEnd(tokenConfig.FieldDelimiter), tokenConfig.Key, tokenConfig.TokenAlgorithm);

            return tokenConfig.PreEscapeAcl ? string.Format("{0}hmac={1}", mToken, hmac) : Uri.EscapeUriString(string.Format("{0}hmac={1}", mToken, hmac));
        }

        private AkamaiTokenConfig InitAkamaiConfig(Dictionary<string, string> dParams)
        {
            AkamaiTokenConfig conf = new AkamaiTokenConfig();

            conf.Acl = "/*";
            conf.TokenAlgorithm = Algorithm.HMACSHA256;           
            conf.Key = m_sSalt;     
            conf.Window = m_nTTL;
            conf.IP = GetIP(dParams);
            conf.StartTime = Convert.ToInt64(Utils.GetEpochUTCTimeNow());

            return conf;
        }

        protected static string GetIP(Dictionary<string, string> dParams)
        {
            return dParams.ContainsKey(Constants.IP) ? dParams[Constants.IP] : string.Empty;
        }

    }
}
