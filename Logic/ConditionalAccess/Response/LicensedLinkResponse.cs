using ApiObjects.ConditionalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
{
    public class LicensedLinkResponse
    {
        public string mainUrl;
        public string altUrl;
        public string status;
        public ApiObjects.Response.Status Status { get; set; }

        public LicensedLinkResponse()
        {
            this.mainUrl = string.Empty;
            this.altUrl = string.Empty;
            this.status = eLicensedLinkStatus.Unknown.ToString();
            Status = new ApiObjects.Response.Status();
        }

        public LicensedLinkResponse(string mainUrl, string altUrl, string status)
        {
            this.mainUrl = mainUrl;
            this.altUrl = altUrl;
            this.status = status;
        }

        public LicensedLinkResponse(string mainUrl, string altUrl, string status, int statusCode, string statusMessage):
            this(mainUrl, altUrl, status)
        {
            Status = new ApiObjects.Response.Status(statusCode, statusMessage);
        }
    }
}
