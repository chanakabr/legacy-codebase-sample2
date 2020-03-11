using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    [Serializable]
    public class PinResponse
    {
        public PinResponse()
        {

        }

        public PinResponse(TVPPro.SiteManager.TvinciPlatform.api.PinResponse copy)
        {
            this.status = new Status(copy.status.Code, copy.status.Message);
            this.pin = copy.pin;
            this.level = ParentalRule.ConvertRuleLevelEnum(copy.level);
        }

        public Status status
        {
            get;
            set;
        }

        public string pin
        {
            get;
            set;
        }

        public eRuleLevel level
        {
            get;
            set;
        }
    }
}
