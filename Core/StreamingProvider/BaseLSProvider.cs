using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Epg;

namespace StreamingProvider
{
    public class BaseLSProvider : ILSProvider
    {
        public static readonly string LIVE_STREAMING_PROVIDER_LOG = "LSProvider";
       
        public BaseLSProvider()
        {
        }

        public virtual string GenerateVODLink(string vodUrl)
        {
            return string.Empty;
        }

        public virtual string GenerateEPGLink(Dictionary<string, object> dParams)
        {
            return GetUrl(dParams);
        }

        protected string GetUrl(Dictionary<string, object> dParams)
        {
            return dParams.ContainsKey("basic_link") ? (string)dParams["basic_link"] : string.Empty;
        }

        protected void ReplaceSubStr(ref string url, Dictionary<string, object> oValuesToReplace)
        {
            if (oValuesToReplace.Count > 0)
            {
                foreach (KeyValuePair<string, object> pair in oValuesToReplace)
                {
                    string sKeyToSearch = string.Format("{0}{1}{2}", "{", pair.Key, "}");
                    if (url.Contains(sKeyToSearch))
                    {
                        url = url.Replace(sKeyToSearch, pair.Value.ToString());
                    }
                }
            }
        }

        protected virtual bool ValidParameters(Dictionary<string, object> dParams)
        {
            if (!dParams.ContainsKey(EpgLinkConstants.BASIC_LINK))
            {
                return false;
            }
            if (string.IsNullOrEmpty(dParams[EpgLinkConstants.BASIC_LINK].ToString()))
            {
                return false;
            }


            if (!dParams.ContainsKey(EpgLinkConstants.PROGRAM_START) || !dParams.ContainsKey(EpgLinkConstants.PROGRAM_END))
            {
                return false;
            }
            if (dParams[EpgLinkConstants.PROGRAM_START] == null || dParams[EpgLinkConstants.PROGRAM_END] == null)
            {
                return false;
            }
            if (!dParams.ContainsKey(EpgLinkConstants.EPG_FORMAT_TYPE))
            {
                return false;
            }
            if (dParams[EpgLinkConstants.EPG_FORMAT_TYPE] == null)
            {
                return false;
            }

            return true;
        }


    }
}
