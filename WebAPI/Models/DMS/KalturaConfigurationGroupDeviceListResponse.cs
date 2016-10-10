using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    /// <summary>
    /// Configuration group devices info wrapper 
    /// </summary>
    [Serializable]
    public class KalturaConfigurationGroupDeviceListResponse : KalturaListResponse
    {
        /// <summary>
        /// Configuration group devices
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaConfigurationGroupDevice> Objects { get; set; }

    }

}