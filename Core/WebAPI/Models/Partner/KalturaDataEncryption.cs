using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public partial class KalturaDataEncryption : KalturaOTTObject
    {
        /// <summary>
        /// Username encryption config
        /// </summary>
        [DataMember(Name = "username")]
        [JsonProperty("username")]
        [XmlElement(ElementName = "username", IsNullable = true)]
        public KalturaEncryption Username { get; set; }
    }
}
