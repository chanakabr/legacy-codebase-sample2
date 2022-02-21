using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Device Reference Data Filter
    /// </summary>
    public partial class KalturaDeviceReferenceDataFilter : KalturaFilter<KalturaDeviceReferenceDataOrderBy>
    {
        /// <summary>
        /// IdIn
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(MinLength = 1)]
        public string IdIn { get; set; }

        public override KalturaDeviceReferenceDataOrderBy GetDefaultOrderByValue()
        {
            return KalturaDeviceReferenceDataOrderBy.NONE;
        }
    }
}