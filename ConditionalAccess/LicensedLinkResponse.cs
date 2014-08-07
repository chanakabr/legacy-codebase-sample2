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

        public LicensedLinkResponse()
        {
            this.mainUrl = string.Empty;
            this.altUrl = string.Empty;
        }

        public LicensedLinkResponse(string mainUrl, string altUrl)
        {
            this.mainUrl = mainUrl;
            this.altUrl = altUrl;
        }
    }
}
