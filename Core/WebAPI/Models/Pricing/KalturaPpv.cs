using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ApiObjects;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// PPV details
    /// </summary>
    public partial class KalturaPpv : KalturaOTTObject
    {
        /// <summary>
        /// PPV identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// the name for the ppv
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// This property will deprecated soon. Please use PriceId instead of it.
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaPriceDetails Price { get; set; }

        /// <summary>
        /// The price if of the ppv
        /// </summary>
        [DataMember(Name = "priceDetailsId")]
        [JsonProperty("priceDetailsId")]
        [XmlElement(ElementName = "priceDetailsId", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public int? PriceDetailsId { get; set; }
        
        /// <summary>
        /// This property will deprecated soon. Please use fileTypesIds instead of it.
        /// </summary>
        [DataMember(Name = "fileTypes")]
        [JsonProperty("fileTypes")]
        [XmlArray(ElementName = "fileTypes", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaIntegerValue> FileTypes { get; set; }

        /// <summary>
        /// Comma separated file types identifiers that are supported in this subscription
        /// </summary>
        [DataMember(Name = "fileTypesIds")]
        [JsonProperty("fileTypesIds")]
        [XmlArray(ElementName = "fileTypesIds", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string FileTypesIds { get; set; }
        
        /// <summary>
        /// This property will deprecated soon. Please use DiscountId instead of it.
        /// </summary>
        [DataMember(Name = "discountModule")]
        [JsonProperty("discountModule")]
        [XmlElement(ElementName = "discountModule", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaDiscountModule DiscountModule { get; set; }

        /// <summary>
        /// The discount id for the ppv
        /// </summary>
        [DataMember(Name = "discountId")]
        [JsonProperty("discountId")]
        [XmlElement(ElementName = "discountId", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? DiscountId { get; set; }
        
        /// <summary>
        /// This property will deprecated soon. Please use CouponsGroupId instead of it.
        /// </summary>
        [DataMember(Name = "couponsGroup")]
        [JsonProperty("couponsGroup")]
        [XmlElement(ElementName = "couponsGroup", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaCouponsGroup CouponsGroup { get; set; }
        
        /// <summary>
        /// Coupons group id for the ppv
        /// </summary>
        [DataMember(Name = "couponsGroupId")]
        [JsonProperty("couponsGroupId")]
        [XmlElement(ElementName = "couponsGroupId", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? CouponsGroupId { get; set; }

        /// <summary>
        /// A list of the descriptions of the ppv on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "descriptions")]
        [JsonProperty("descriptions")]
        [XmlArray(ElementName = "descriptions", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(IsNullable = true)]
        public List<KalturaTranslationToken> Descriptions { get; set; } // TODO: change to object

        /// <summary>
        /// Product code for the ppv
        /// </summary>
        [DataMember(Name = "productCode")]
        [JsonProperty("productCode")]
        [XmlElement(ElementName = "productCode")]
        [SchemeProperty(IsNullable = true)]
        public string ProductCode { get; set; }

        /// <summary>
        /// Indicates whether or not this ppv can be purchased standalone or only as part of a subscription
        /// </summary>
        [DataMember(Name = "isSubscriptionOnly")]
        [JsonProperty("isSubscriptionOnly")]
        [XmlElement(ElementName = "isSubscriptionOnly", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public bool? IsSubscriptionOnly { get; set; }

        /// <summary>
        /// Indicates whether or not this ppv can be consumed only on the first device
        /// </summary>
        [DataMember(Name = "firstDeviceLimitation")]
        [JsonProperty("firstDeviceLimitation")]
        [XmlElement(ElementName = "firstDeviceLimitation", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public bool? FirstDeviceLimitation { get; set; }

        /// <summary>
        ///  This property will deprecated soon. Please use UsageModuleId instead of it.
        /// </summary>
        [DataMember(Name = "usageModule")]
        [JsonProperty("usageModule")]
        [XmlElement(ElementName = "usageModule", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public KalturaUsageModule UsageModule { get; set; }

        /// <summary>
        /// PPV usage module Id
        /// </summary>
        [DataMember(Name = "usageModuleId")]
        [JsonProperty("usageModuleId")]
        [XmlElement(ElementName = "usageModuleId", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? UsageModuleId { get; set; }
        
        /// <summary>
        /// adsPolicy
        /// </summary>
        [DataMember(Name = "adsPolicy")]
        [JsonProperty("adsPolicy")]
        [XmlElement(ElementName = "adsPolicy")]
        [SchemeProperty(IsNullable = true)]
        public KalturaAdsPolicy? AdsPolicy { get; set; }
        
        /// <summary>
        /// Is active ppv
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive", IsNullable = true)]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public bool? IsActive { get; set; }
        
        /// <summary>
        /// Specifies when was the ppv last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// Specifies when was the ppv created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Virtual asset id
        /// </summary>
        [DataMember(Name = "virtualAssetId")]
        [JsonProperty("virtualAssetId")]
        [XmlElement(ElementName = "virtualAssetId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? VirtualAssetId { get; set; }

        /// <summary>
        /// Asset user rule identifier 
        /// </summary>
        [DataMember(Name = "assetUserRuleId")]
        [JsonProperty("assetUserRuleId")]
        [XmlElement(ElementName = "assetUserRuleId")]
        [SchemeProperty(RequiresPermission = (int)RequestType.INSERT, IsNullable = true)]
        public long? AssetUserRuleId { get; set; }

        internal List<int> GetFileTypesIds()
        {
            if (FileTypesIds != null && FileTypesIds == "")
            {
                return new List<int>();
            }
            else if(FileTypesIds == null)
            {
                return null;
            }
            else
            {
                return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(FileTypesIds, "KalturaSubscription.FileTypesIds", true);
            }
        }
        
        internal void ValidateForAdd()
        {
            if (string.IsNullOrEmpty(Name))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            if (!IsSubscriptionOnly.HasValue || (IsSubscriptionOnly.HasValue && !IsSubscriptionOnly.Value))
            {
                if (!PriceDetailsId.HasValue  || PriceDetailsId.Value == 0)
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "PriceId");
                if (!UsageModuleId.HasValue || UsageModuleId.Value == 0)
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "UsageModuleId");
            }
        }
    }
}