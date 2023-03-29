using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Program asset group offer filter
    /// </summary>
    [SchemeClass(OneOf = new[] { "idIn", "nameContains" })]
    public partial class KalturaProgramAssetGroupOfferIdInFilter : KalturaProgramAssetGroupOfferFilter
    {
        /// <summary>
        /// Comma separated asset group offer identifiers        
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = false)]
        [SchemeProperty(DynamicMinInt = 1)]
        public string IdIn { get; set; }
    }
}