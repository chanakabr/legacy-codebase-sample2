using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Purchase settings and PIN
    /// </summary>
    [Serializable]
    public class KalturaPurchaseSettingsResponse : KalturaOTTObject
    {
        /// <summary>
        /// Purchase settings type - block, ask or allow
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement(ElementName = "type")]
        public KalturaPurchaseSettingsType type
        {
            get;
            set;
        }

        /// <summary>
        /// PIN applied on user or household
        /// </summary>
        [DataMember(Name = "pin")]
        [JsonProperty(PropertyName = "pin", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement(ElementName = "pin")]
        public string pin
        {
            get;
            set;
        }

        /// <summary>
        /// Where were these settings defined - account, household or user
        /// </summary>
        [DataMember(Name = "origin")]
        [JsonProperty(PropertyName = "origin")]
        [XmlElement(ElementName = "origin")]
        public KalturaRuleLevel origin
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
