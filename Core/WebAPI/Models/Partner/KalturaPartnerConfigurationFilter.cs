using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{

    /// <summary>
    /// Partner configuration filter 
    /// </summary>
    public partial class KalturaPartnerConfigurationFilter : KalturaFilter<KalturaPartnerConfigurationOrderBy>
    {
        /// <summary>
        /// Indicates which partner configuration list to return
        /// </summary>
        [DataMember(Name = "partnerConfigurationTypeEqual")]
        [JsonProperty("partnerConfigurationTypeEqual")]
        [XmlElement(ElementName = "partnerConfigurationTypeEqual")]
        public KalturaPartnerConfigurationType PartnerConfigurationTypeEqual { get; set; }

        public override KalturaPartnerConfigurationOrderBy GetDefaultOrderByValue()
        {
            return KalturaPartnerConfigurationOrderBy.NONE;
        }
    }
}