using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Partner;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Parameters for partner setup
    /// </summary>
    public partial class KalturaPartnerSetup : KalturaOTTObject
    {
        /// <summary>
        /// admin Username
        /// </summary>
        [DataMember(Name = "adminUsername")]
        [JsonProperty(PropertyName = "adminUsername")]
        [XmlElement("adminUsername")]
        [SchemeProperty(MinLength = 1, MaxLength = 256)]
        public string AdminUsername { get; set; }

        /// <summary>
        /// admin Password
        /// </summary>
        [DataMember(Name = "adminPassword")]
        [JsonProperty(PropertyName = "adminPassword")]
        [XmlElement("adminPassword")]
        [SchemeProperty(MinLength = 1, MaxLength = 128)]
        public string AdminPassword { get; set; }

        /// <summary>
        /// basePartnerConfiguration
        /// </summary>
        [DataMember(Name = "basePartnerConfiguration")]
        [JsonProperty(PropertyName = "basePartnerConfiguration")]
        [XmlElement("basePartnerConfiguration")]
        public KalturaBasePartnerConfiguration BasePartnerConfiguration { get; set; }
    }
}