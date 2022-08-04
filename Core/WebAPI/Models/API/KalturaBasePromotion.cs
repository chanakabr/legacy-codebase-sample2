using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Base Promotion
    /// </summary>
    public abstract partial class KalturaBasePromotion : KalturaOTTObject
    {
        /// <summary>
        /// These conditions define the Promotion that applies on
        /// </summary>
        [DataMember(Name = "conditions")]
        [JsonProperty("conditions")]
        [XmlElement(ElementName = "conditions")]
        public List<KalturaCondition> Conditions { get; set; }
    }
}