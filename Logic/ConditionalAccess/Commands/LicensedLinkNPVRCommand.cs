using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Response;
using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
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
            LicensedLinkResponse licensedLinkResponse = cas.GetEPGLink(assetID, startTime, format, siteGuid, mediaFileID, basicLink, userIP, referrer, countryCd, langCd, udid, couponCode);
            if (licensedLinkResponse.status == "OK" && !string.IsNullOrEmpty(licensedLinkResponse.mainUrl))
            {
                res.status = NPVRStatus.OK.ToString();
                res.mainUrl = licensedLinkResponse.mainUrl;
            }
            else if (licensedLinkResponse.status == "ServiceNotAllowed")
            {
                res.status = NPVRStatus.ServiceNotAllowed.ToString();
                res.msg = "NPVR service is not allowed";
            }
            else if (licensedLinkResponse.status == eResponseStatus.DomainSuspended.ToString())
            {
                res.status = NPVRStatus.Suspended.ToString();
            }
            else
            {
                res.status = NPVRStatus.Error.ToString();
            }

            return res;
        }
    }
}
