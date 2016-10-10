using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS 
{
    /// <summary>
    /// Configuration groups info wrapper 
    /// </summary>
    [Serializable]
    public class KalturaConfigurationGroupListResponse : KalturaListResponse
    {
        /// <summary>
        /// Configuration groups
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaConfigurationGroup> Objects { get; set; }

    }

}