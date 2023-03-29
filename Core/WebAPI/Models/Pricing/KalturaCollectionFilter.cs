using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Collection Filter
    /// </summary>
    [SchemeClass(OneOf = new[] { "collectionIdIn", "mediaFileIdEqual", "nameContains" })]
    public partial class KalturaCollectionFilter : KalturaFilter<KalturaCollectionOrderBy>
    {
        /// <summary>
        /// Comma separated collection IDs
        /// </summary>
        [DataMember(Name = "collectionIdIn")]
        [JsonProperty("collectionIdIn")]
        [XmlElement(ElementName = "collectionIdIn")]
        [SchemeProperty(MinLength = 1)]
        public string CollectionIdIn { get; set; }

        /// <summary>
        /// Media-file ID to get the collections by
        /// </summary>
        [DataMember(Name = "mediaFileIdEqual")]
        [JsonProperty("mediaFileIdEqual")]
        [XmlElement(ElementName = "mediaFileIdEqual", IsNullable = true)]
        public int? MediaFileIdEqual { get; set; }

        /// <summary>
        /// couponGroupIdEqual
        /// </summary>
        [DataMember(Name = "couponGroupIdEqual")]
        [JsonProperty("couponGroupIdEqual")]
        [XmlElement(ElementName = "couponGroupIdEqual", IsNullable = true)]
        [SchemeProperty(MinInteger = 1)]
        public int? CouponGroupIdEqual { get; set; }

        /// <summary>
        ///  return also inactive 
        /// </summary>
        [DataMember(Name = "alsoInactive")]
        [JsonProperty("alsoInactive")]
        [XmlElement(ElementName = "alsoInactive")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public bool? AlsoInactive { get; set; }
        
        /// <summary>
        /// comma-separated list of KalturaCollection.assetUserRuleId values.  Matching KalturaCollection objects will be returned by the filter.
        /// </summary>
        [DataMember(Name = "assetUserRuleIdIn")]
        [JsonProperty("assetUserRuleIdIn")]
        [XmlElement(ElementName = "assetUserRuleIdIn", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ, IsNullable = true, DynamicMinInt = 1)]
        public string AssetUserRuleIdIn { get; set; }

        /// <summary>
        /// A string that is included in the collection name
        /// </summary>
        [DataMember(Name = "nameContains")]
        [JsonProperty("nameContains")]
        [XmlElement(ElementName = "nameContains")]
        [SchemeProperty(IsNullable = true, MinLength = 1, MaxLength = 50, RequiresPermission = (int)RequestType.READ)]
        public string NameContains { get; set; }

        public override KalturaCollectionOrderBy GetDefaultOrderByValue()
        {
            return KalturaCollectionOrderBy.NONE;
        }
    }
}