using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public partial class KalturaParentalRuleFilter : KalturaFilter<KalturaParentalRuleOrderBy>
    {
        /// <summary>
        ///Reference type to filter by
        /// </summary>
        [DataMember(Name = "entityReferenceEqual")]
        [JsonProperty("entityReferenceEqual")]
        [XmlElement(ElementName = "entityReferenceEqual")]
        public KalturaEntityReferenceBy? EntityReferenceEqual { get; set; }

        public override KalturaParentalRuleOrderBy GetDefaultOrderByValue()
        {
            return KalturaParentalRuleOrderBy.PARTNER_SORT_VALUE;
        }
    }
}