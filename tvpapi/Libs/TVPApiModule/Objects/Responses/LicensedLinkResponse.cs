using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class LicensedLinkResponse
    {
        [JsonProperty(PropertyName = "licensed_link")] 
        public LicensedLink LicensedLink { get; set; }

        [JsonProperty(PropertyName = "status")] 
        public Status Status { get; set; }

        public LicensedLinkResponse(TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.LicensedLinkResponse licensedLink)
        {
            LicensedLink = new LicensedLink()
            {
                MainUrl = licensedLink.mainUrl,
                AlternateUrl = licensedLink.altUrl
            };

            Status = new Status()
            {
                Code = licensedLink.Status.Code,
                Message = licensedLink.Status.Message
            };
        }

        public LicensedLinkResponse()
        {
        }
    }
}
