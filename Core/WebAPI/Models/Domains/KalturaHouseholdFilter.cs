using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{

    /// <summary>
    /// Household details
    /// </summary>
    public partial class KalturaHouseholdFilter : KalturaFilter<KalturaHouseholdOrderBy>
    {
        /// <summary>
        /// Household external identifier to search by
        /// </summary>
        [DataMember(Name = "externalIdEqual")]
        [JsonProperty("externalIdEqual")]
        [XmlElement(ElementName = "externalIdEqual")]
        public string ExternalIdEqual { get; set; }

        public override KalturaHouseholdOrderBy GetDefaultOrderByValue()
        {
            return KalturaHouseholdOrderBy.CREATE_DATE_DESC;
        }
    }
}