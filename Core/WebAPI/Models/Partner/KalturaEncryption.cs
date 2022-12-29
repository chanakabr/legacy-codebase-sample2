using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public partial class KalturaEncryption : KalturaOTTObject
    {
        /// <summary>
        /// Encryption type
        /// </summary>
        [DataMember(Name = "encryptionType")]
        [JsonProperty("encryptionType")]
        [XmlElement(ElementName = "encryptionType")]
        public KalturaEncryptionType EncryptionType { get; set; }
    }
}
