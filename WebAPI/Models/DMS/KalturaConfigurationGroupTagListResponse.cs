using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS 
{
    /// <summary>
    /// Configurations group tags info wrapper
    /// </summary>
    [Serializable]
    public class KalturaConfigurationGroupTagListResponse : KalturaListResponse
    {
        /// <summary>
        /// Configuration group tags
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaConfigurationGroupTag> Objects { get; set; }

    }

}