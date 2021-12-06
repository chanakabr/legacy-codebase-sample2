using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Collection Filter
    /// </summary>
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

        public override KalturaCollectionOrderBy GetDefaultOrderByValue()
        {
            return KalturaCollectionOrderBy.NONE;
        }
    }
}