using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.Domains;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public partial class KalturaDeviceBrandFilter : KalturaFilter<KalturaDeviceBrandOrderBy>
    {
        /// <summary>
        /// Filter the device brand with this identifier.
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement("idEqual")]
        public long? IdEqual { get; set; }

        /// <summary>
        /// Filter the device brands with this device family's identifier.
        /// </summary>
        [DataMember(Name = "deviceFamilyIdEqual")]
        [JsonProperty("deviceFamilyIdEqual")]
        [XmlElement("deviceFamilyIdEqual")]
        public long? DeviceFamilyIdEqual { get; set; }

        /// <summary>
        /// Filter the device brand with this name.
        /// </summary>
        [DataMember(Name = "nameEqual")]
        [JsonProperty("nameEqual")]
        [XmlElement("nameEqual")]
        public string NameEqual { get; set; }

        /// <summary>
        /// Filter device brands of this type
        /// </summary>
        [DataMember(Name = "typeEqual")]
        [JsonProperty("typeEqual")]
        [XmlElement("typeEqual")]
        public KalturaDeviceBrandType? TypeEqual { get; set; }

        public override KalturaDeviceBrandOrderBy GetDefaultOrderByValue()
        {
            return KalturaDeviceBrandOrderBy.ID_ASC;
        }
    }
}