using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// IOT PROFILE
    /// </summary>
    public partial class KalturaIotProfile : KalturaOTTObjectSupportNullable
    {
        /// <summary>
        /// adapterUrl
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty(PropertyName = "adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        public string AdapterUrl { get; set; }

        /// <summary>
        /// kalturaIotProfileAws
        /// </summary>
        [DataMember(Name = "iotProfileAws")]
        [JsonProperty(PropertyName = "iotProfileAws")]
        [XmlElement(ElementName = "iotProfileAws")]
        public KalturaIotProfileAws IotProfileAws { get; set; }
    }
}
