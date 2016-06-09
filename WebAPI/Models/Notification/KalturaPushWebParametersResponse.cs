using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Name = "KalturaPushWebParametersResponse", Namespace = "")]
    [XmlRoot("KalturaPushWebParametersResponse")]
    public class KalturaPushWebParametersResponse : KalturaListResponse
    {
        /// push web parameters
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaPushWebParameters> PushWebParameters { get; set; }
    }
}