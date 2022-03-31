using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Models.Domains;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public partial class KalturaDeviceFamilyFilter : KalturaFilter<KalturaDeviceFamilyOrderBy>
    {
        /// <summary>
        /// Filter the device family with this identifier.
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement("idEqual")]
        public long? IdEqual { get; set; }

        /// <summary>
        /// Filter the device family with this name.
        /// </summary>
        [DataMember(Name = "nameEqual")]
        [JsonProperty("nameEqual")]
        [XmlElement("nameEqual")]
        public string NameEqual { get; set; }
        
        /// <summary>
        /// Filter device families of this type
        /// </summary>
        [DataMember(Name = "typeEqual")]
        [JsonProperty("typeEqual")]
        [XmlElement("typeEqual")]
        public KalturaDeviceFamilyType? TypeEqual { get; set; }

        public override KalturaDeviceFamilyOrderBy GetDefaultOrderByValue()
        {
            return KalturaDeviceFamilyOrderBy.ID_ASC;
        }
    }
}