using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class PinCodeResponse
    {
        public ApiObjects.Response.Status resp { get; set; }

        public string pinCode { get; set; }
        public DateTime expiredDate { get; set; }
        public string siteGuid { get; set; }


        public PinCodeResponse()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            pinCode = string.Empty;
            expiredDate = DateTime.MinValue;
            siteGuid = string.Empty;
        }
        public PinCodeResponse(ApiObjects.Response.Status resp, string pinCode, DateTime expiredDate, string siteGuid)
        {
            this.resp = resp;
            this.pinCode = pinCode;
            this.expiredDate = expiredDate;
            this.siteGuid = siteGuid;
        }
    }
}
