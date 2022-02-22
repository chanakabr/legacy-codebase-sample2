using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    /// <summary>
    /// representation of duration time unit and value 
    /// </summary>
    public partial class KalturaDuration : KalturaOTTObject
    {
        /// <summary>
        /// duration unit
        /// </summary>
        [DataMember(Name = "unit")]
        [JsonProperty("unit")]
        [XmlElement(ElementName = "unit")]
        public KalturaDurationUnit Unit { get; set; }

        /// <summary>
        /// duration value 
        /// </summary>
        [DataMember(Name = "value")]
        [XmlElement("value")]
        [JsonProperty("value")]
        [SchemeProperty(MinInteger = 1)]
        public int Value { get; set; }

        /// <summary>
        /// duration code - the canculat time in minutes except from years and months that have specific code 
        /// </summary>
        [DataMember(Name = "code")]
        [XmlElement("code")]
        [JsonProperty("code")]
        [SchemeProperty(ReadOnly = true)]
        public long Code { get; set; }
    }
}