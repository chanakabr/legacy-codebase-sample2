using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class LicensedLinkResponse
    {
        public string mainUrl;
        public string altUrl;
        public string status;

        public LicensedLinkResponse()
        {
            this.mainUrl = string.Empty;
            this.altUrl = string.Empty;
            this.status = eLicensedLinkStatus.Unknown.ToString();
        }

        public LicensedLinkResponse(string mainUrl, string altUrl, string status)
        {
            this.mainUrl = mainUrl;
            this.altUrl = altUrl;
            this.status = status;
        }
    }
}
