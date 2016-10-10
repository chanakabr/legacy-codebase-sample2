using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS 
{
    /// <summary>
    /// Configurations info wrapper
    /// </summary>
    [Serializable]
    public class KalturaConfigurationListResponse : KalturaListResponse
    {
        /// <summary>
        /// Configurations
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaConfiguration> Objects { get; set; }

    }

}