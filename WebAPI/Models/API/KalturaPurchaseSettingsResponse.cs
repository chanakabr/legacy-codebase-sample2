using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Purchase settings and PIN
    /// </summary>
    [Serializable]
    [OldStandard("purchaseSettingsType", "purchase_settings_type")]
    [Obsolete]
    public class KalturaPurchaseSettingsResponse : KalturaPinResponse
    {
        /// <summary>
        /// Purchase settings type - block, ask or allow
        /// </summary>
        [DataMember(Name = "purchaseSettingsType")]
        [JsonProperty(PropertyName = "purchaseSettingsType", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement(ElementName = "purchaseSettingsType", IsNullable = true)]
        public KalturaPurchaseSettingsType? PurchaseSettingsType
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
    public enum KalturaPurchaseSettingsType
    {
        block = 0,
        ask = 1,
        allow = 2
    }
}
