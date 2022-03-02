using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Program asset group offer filter
    /// </summary>
    public partial class KalturaProgramAssetGroupOfferFilter : KalturaFilter<KalturaProgramAssetGroupOfferOrderBy>
    {
        /// <summary>
        ///  return also inactive 
        /// </summary>
        [DataMember(Name = "alsoInactive")]
        [JsonProperty("alsoInactive")]
        [XmlElement(ElementName = "alsoInactive")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public bool? AlsoInactive { get; set; }

        public override KalturaProgramAssetGroupOfferOrderBy GetDefaultOrderByValue()
        {
            return KalturaProgramAssetGroupOfferOrderBy.NAME_ASC;
        }
    }
}