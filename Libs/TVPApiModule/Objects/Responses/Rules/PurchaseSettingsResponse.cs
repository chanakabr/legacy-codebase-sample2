using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class PurchaseSettingsResponse
    {
        public PurchaseSettingsResponse()
        {

        }

        public PurchaseSettingsResponse(ApiObjects.PurchaseSettingsResponse copy)
        {
            this.status = new Status(copy.status.Code, copy.status.Message);

            if (this.status.Code != 0)
            {
                this.level = null;
                this.type = null;
            }
            else
            {
                switch (copy.type)
                {
                    case ApiObjects.ePurchaeSettingsType.Block:
                    {
                        this.type = ePurchaeSettingsType.Block;
                        break;
                    }
                    case ApiObjects.ePurchaeSettingsType.Ask:
                    {
                        this.type = ePurchaeSettingsType.Ask;
                        break;
                    }
                    case ApiObjects.ePurchaeSettingsType.Allow:
                    {
                        this.type = ePurchaeSettingsType.Allow;
                        break;
                    }
                    default:
                    {
                        this.type = ePurchaeSettingsType.Block;
                        break;
                    }
                }

                this.pin = copy.pin;
                this.level = ParentalRule.ConvertRuleLevelEnum(copy.level);
            }
        }

        public Status status
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "type", NullValueHandling= NullValueHandling.Ignore)]
        public ePurchaeSettingsType? type
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "pin", NullValueHandling = NullValueHandling.Ignore)]
        public string pin
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "level", NullValueHandling = NullValueHandling.Ignore)]
        public eRuleLevel? level
        {
            get;
            set;
        }
    }

    /// <summary>
    /// One of the following options:
    /// -	Block – purchases not allowed
    /// -	Ask – allow purchase subject to purchase PIN
    /// -	Allow – allow purchases with no purchase PIN
    /// </summary>
    public enum ePurchaeSettingsType
    {
        Block = 0,
        Ask = 1,
        Allow = 2
    }
}
