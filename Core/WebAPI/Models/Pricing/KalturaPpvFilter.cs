using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public enum KalturaPpvOrderBy
    {
        NAME_ASC,
        NAME_DESC,
        UPDATE_DATE_ASC,
        UPDATE_DATE_DESC
    }

    /// <summary>
    /// Filtering Asset Struct Metas
    /// </summary>
    [Serializable]
    public partial class KalturaPpvFilter : KalturaFilter<KalturaPpvOrderBy>
    {
        /// <summary>
        /// Comma separated identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        public string IdIn { get; set; }
        
        /// <summary>
        /// couponGroupIdEqual
        /// </summary>
        [DataMember(Name = "couponGroupIdEqual")]
        [JsonProperty("couponGroupIdEqual")]
        [XmlElement(ElementName = "couponGroupIdEqual", IsNullable = true)]
        public int? CouponGroupIdEqual { get; set; }

        /// <summary>
        /// return also inactive 
        /// </summary>
        [DataMember(Name = "alsoInactive")]
        [JsonProperty("alsoInactive")]
        [XmlElement(ElementName = "alsoInactive", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public bool? AlsoInactive { get; set; }

        /// <summary>
        /// comma-separated list of KalturaPpv.assetUserRuleId values.  Matching KalturaPpv objects will be returned by the filter.
        /// </summary>
        [DataMember(Name = "assetUserRuleIdIn")]
        [JsonProperty("assetUserRuleIdIn")]
        [XmlElement(ElementName = "assetUserRuleIdIn", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ, IsNullable = true, DynamicMinInt = 1)]
        public string AssetUserRuleIdIn { get; set; }

        public override KalturaPpvOrderBy GetDefaultOrderByValue()
        {
            return KalturaPpvOrderBy.NAME_ASC;
        }
    }
}