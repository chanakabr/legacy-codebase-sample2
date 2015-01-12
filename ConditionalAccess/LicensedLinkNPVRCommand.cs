using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class LicensedLinkNPVRCommand : BaseNPVRCommand
    {
        public DateTime startTime;
        public int format;
        public int mediaFileID;
        public string basicLink;
        public string userIP;
        public string referrer;
        public string countryCd;
        public string langCd;
        public string couponCode;

        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            LicensedLinkNPVRResponse res = new LicensedLinkNPVRResponse();
            res.mainUrl = cas.GetEPGLink(assetID, startTime, format, siteGuid, mediaFileID, basicLink, userIP, referrer, countryCd, langCd,
                udid, couponCode);
            if (!string.IsNullOrEmpty(res.mainUrl))
            {
                res.status = NPVRStatus.OK.ToString();
            }
            else
            {
                res.status = NPVRStatus.Error.ToString();
            }

            return res;
        }
    }
}
