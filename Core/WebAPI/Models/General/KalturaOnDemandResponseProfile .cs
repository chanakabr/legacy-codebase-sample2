using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Define on demand response 
    /// </summary>
    [JsonObject]
    public partial class KalturaOnDemandResponseProfile : KalturaDetachedResponseProfile
    {
        /// <summary>
        /// Comma seperated properties names 
        /// </summary>
        [DataMember(Name = "retrievedProperties")]
        [JsonProperty("retrievedPropertiesretrievedProperties")]
        [XmlElement(ElementName = "retrievedProperties")]
        public string RetrievedProperties { get; set; }
    }
}