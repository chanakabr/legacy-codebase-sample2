using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    public partial class KalturaPartnerFilter : KalturaFilter<KalturaPartnerFilterOrderBy>
    {
        /// <summary>
        /// Comma separated discount codes
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        [SchemeProperty(DynamicMinInt = 1)]
        public string IdIn { get; set; }

        public override KalturaPartnerFilterOrderBy GetDefaultOrderByValue()
        {
            return KalturaPartnerFilterOrderBy.CODE_ASC;
        }
    }
}