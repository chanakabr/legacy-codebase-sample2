using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Preview module
    /// </summary>
    [OldStandard("lifeCycle", "life_cycle")]
    [OldStandard("nonRenewablePeriod", "non_renewable_period")]
    public class KalturaPreviewModule : KalturaOTTObject
    {
        /// <summary>
        /// Preview module identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
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
        [DataMember(Name = "lifeCycle")]
        [JsonProperty("lifeCycle")]
        [XmlElement(ElementName = "lifeCycle")]
        public int? LifeCycle { get; set; }

        /// <summary>
        /// The time you can't buy the item to which the preview module is assigned to again
        /// </summary>
        [DataMember(Name = "nonRenewablePeriod")]
        [JsonProperty("nonRenewablePeriod")]
        [XmlElement(ElementName = "nonRenewablePeriod")]
        public int? NonRenewablePeriod { get; set; }

        
    }
}