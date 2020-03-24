using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class PinCodeResponse
    {
        [JsonProperty(PropertyName = "result")]
        public PinCode Result { get; set; }

        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }

        public PinCodeResponse(Core.Users.PinCodeResponse pinCode)
        {
            if (pinCode != null)
            {
                this.Status = new Responses.Status(pinCode.resp.Code, pinCode.resp.Message);
                this.Result = new PinCode(pinCode);                
            }
        }

        public PinCodeResponse()
        {           
        }

    }


    public class PinCode
    {
        [JsonProperty(PropertyName = "pin_code")]
        public string pinCode { get; set; }
        [JsonProperty(PropertyName = "expired_date")]
        public DateTime expiredDate { get; set; }
        [JsonProperty(PropertyName = "site_guid")]
        public string siteGuid { get; set; }

        public PinCode(Core.Users.PinCodeResponse pinCode)
        {
            this.pinCode = pinCode.pinCode;
            this.expiredDate = pinCode.expiredDate;
            this.siteGuid = pinCode.siteGuid;           
        }
    }
}
