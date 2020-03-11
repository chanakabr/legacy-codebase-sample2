using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CDNTokenizers.Tokenizers
{
    public class MediaValutTokenizer : BaseCDNTokenizer
    {
        public MediaValutTokenizer(int nGroupID, int nStreamingCompanyID)
            : base(nGroupID, nStreamingCompanyID)
        {
        }

        public override string GenerateToken(Dictionary<string,string> dParams)
        {
            string resultURL = string.Empty;
            try
            {
                MediaVaultOptions options = InitMediaValutOptions(dParams);
                resultURL = Compute(options);
            }
            catch { }

            return resultURL;
        }

        protected string Compute(MediaVaultOptions options)
        {
            if (string.IsNullOrEmpty(options.VideoUri.PathAndQuery))
            {
                throw new Exception("video url is required.");
            }

            string result = options.VideoUri.AbsoluteUri;
            string urlParams = string.Empty;
            string hash = string.Empty;

            if (!string.IsNullOrEmpty(options.Referrer))
            {
                Uri u = new Uri(options.Referrer);
                urlParams += "&ru=" + (u.Scheme + "://" + u.Host).Length.ToString();
                hash = options.Referrer;
            }

            if (!string.IsNullOrEmpty(options.PageURL))
            {
                urlParams += "&pu=" + options.PageURL.Length.ToString();
                hash += options.PageURL;
            }
            if (options.StartTime != null) urlParams += string.Concat("&s=", options.StartTime);
            if (options.EndTime != null) urlParams += string.Concat("&e=", options.EndTime);
            if (!string.IsNullOrEmpty(options.IPAddress)) urlParams += string.Concat("&ip=", options.IPAddress);

            if (!string.IsNullOrEmpty(urlParams))
            {
                urlParams = urlParams.Remove(0, 1);
                if (result.Contains("?"))
                {
                    result += "&" + urlParams;
                }
                else
                {
                    result += "?" + urlParams;
                }
            }

            hash = Utils.GetMD5Hash(this.m_sSalt + hash + result);

            result += (result.Contains("?") ? "&h=" + hash : "?h=" + hash);

            return result;
        }

        #region private
        private MediaVaultOptions InitMediaValutOptions(Dictionary<string, string> dParams)
        {
            MediaVaultOptions options = new MediaVaultOptions();
            options.StartTime = Utils.GetEpochUTCTimeNow() - 3600;
            options.EndTime = Utils.GetEpochUTCTimeNow() + m_nTTL;

            string ip;
            if (dParams.TryGetValue(Constants.IP, out ip))
            {
                options.IPAddress = ip;
            }

            options.VideoUri = new Uri(dParams[Constants.URL]);

            return options;

        }

        protected class MediaVaultOptions
        {
            public long? StartTime { get; set; }
            public long? EndTime { get; set; }
            public string IPAddress { get; set; }
            public string Referrer { get; set; }
            public string PageURL { get; set; }
            public Uri VideoUri { get; set; }

            public MediaVaultOptions()
            {
                IPAddress = "";
                Referrer = "";
                PageURL = "";
            }
        }
        #endregion
    }
}
