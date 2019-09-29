using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Purchase settings and PIN
    /// </summary>
    [Serializable]
    public partial class KalturaPurchaseSettings : KalturaPin
    {
        /// <summary>
        /// Purchase permission - block, ask or allow
        /// </summary>
        [DataMember(Name = "permission")]
        [JsonProperty(PropertyName = "permission", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement(ElementName = "permission", IsNullable = true)]
        public KalturaPurchaseSettingsType? Permission
        {
            get;
            set;
        }
    }
}
