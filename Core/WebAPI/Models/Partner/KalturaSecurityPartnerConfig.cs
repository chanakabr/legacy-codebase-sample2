using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Partner
{
    public partial class KalturaSecurityPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// Encryption config
        /// </summary>
        [DataMember(Name = "encryption")]
        [JsonProperty("encryption")]
        [XmlElement(ElementName = "encryption", IsNullable = true)]
        public KalturaDataEncryption Encryption { get; set; }
    }
}
