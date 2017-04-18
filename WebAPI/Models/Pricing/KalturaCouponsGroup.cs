using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Coupons group details
    /// </summary>
    public class KalturaCouponsGroup : KalturaOTTObject
    {
        /// <summary>
        /// Coupon group identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public string Id { get; set; }

        /// <summary>
        /// Coupon group name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// A list of the descriptions of the coupon group on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "descriptions")]
        [JsonProperty("descriptions")]
        [XmlArray(ElementName = "descriptions", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTranslationToken> Descriptions { get; set; }

        /// <summary>
        /// The first date the coupons in this coupons group are valid
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate", IsNullable = true)]
        [OldStandardProperty("start_date")]
        public long? StartDate { get; set; }

        /// <summary>
        /// The last date the coupons in this coupons group are valid
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate", IsNullable = true)]
        [OldStandardProperty("end_date")]
        public long? EndDate { get; set; }

        /// <summary>
        /// Maximum number of uses for each coupon in the group
        /// </summary>
        [DataMember(Name = "maxUsesNumber")]
        [JsonProperty("maxUsesNumber")]
        [XmlElement(ElementName = "maxUsesNumber", IsNullable = true)]
        [OldStandardProperty("max_uses_number")]
        public int? MaxUsesNumber { get; set; }

        /// <summary>
        /// Maximum number of uses for each coupon in the group on a renewable subscription
        /// </summary>
        [DataMember(Name = "maxUsesNumberOnRenewableSub")]
        [JsonProperty("maxUsesNumberOnRenewableSub")]
        [XmlElement(ElementName = "maxUsesNumberOnRenewableSub", IsNullable = true)]
        [OldStandardProperty("max_uses_number_on_renewable_sub")]
        public int? MaxUsesNumberOnRenewableSub { get; set; }


        /// <summary>
        /// Type of the coupon group
        /// </summary>
        [DataMember(Name = "couponGroupType")]
        [JsonProperty("couponGroupType")]
        [XmlElement(ElementName = "couponGroupType", IsNullable = true)]
        public KalturaCouponGroupType? CouponGroupType
        {
            get;
            set;
        }
    }
}