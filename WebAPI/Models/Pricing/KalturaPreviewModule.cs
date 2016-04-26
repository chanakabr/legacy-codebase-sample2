using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Preview module
    /// </summary>
    public class KalturaPreviewModule : KalturaOTTObject
    {
        /// <summary>
        /// Preview module identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long? Id { get; set; }

        /// <summary>
        /// Preview module name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Preview module life cycle - for how long the preview module is active
        /// </summary>
        [DataMember(Name = "life_cycle")]
        [JsonProperty("life_cycle")]
        [XmlElement(ElementName = "life_cycle")]
        public int? LifeCycle { get; set; }

        /// <summary>
        /// The time you can't buy the item to which the preview module is assigned to again
        /// </summary>
        [DataMember(Name = "non_renewable_period")]
        [JsonProperty("non_renewable_period")]
        [XmlElement(ElementName = "non_renewable_period")]
        public int? NonRenewablePeriod { get; set; }

        
    }
}