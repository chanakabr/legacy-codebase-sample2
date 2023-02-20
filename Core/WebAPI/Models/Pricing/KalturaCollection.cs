using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Collection
    /// </summary>
    public partial class KalturaCollection : KalturaOTTObjectSupportNullable
    {
        /// <summary>
        /// Collection identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// A list of channels associated with this collection 
        /// This property will deprecated soon. Please use ChannelsIds instead of it.
        /// </summary>
        [DataMember(Name = "channels")]
        [JsonProperty("channels")]
        [XmlArray(ElementName = "channels", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public List<KalturaBaseChannel> Channels { get; set; }

        /// <summary>
        /// Comma separated channels Ids associated with this collection
        /// </summary>
        [DataMember(Name = "channelsIds")]
        [JsonProperty("channelsIds")]
        [XmlArray(ElementName = "channelsIds", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string ChannelsIds { get; set; }

        /// <summary>
        /// The first date the collection is available for purchasing 
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate")]
        [SchemeProperty(IsNullable = true)]
        public long? StartDate { get; set; }

        /// <summary>
        /// The last date the collection is available for purchasing
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate")]
        [SchemeProperty(IsNullable = true)]
        public long? EndDate { get; set; }

        /// <summary>        
        /// The internal discount module for the collection
        /// This property will deprecated soon. Please use DiscountModuleId instead of it.
        /// </summary>
        [DataMember(Name = "discountModule")]
        [JsonProperty("discountModule")]
        [XmlElement(ElementName = "discountModule", IsNullable = true)]
        [OldStandardProperty("discount_module")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public KalturaDiscountModule DiscountModule { get; set; }

        /// <summary>
        /// The internal discount module identifier for the collection
        /// </summary>
        [DataMember(Name = "discountModuleId")]
        [JsonProperty("discountModuleId")]
        [XmlElement(ElementName = "discountModuleId", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLong = 1)]
        public long? DiscountModuleId { get; set; }

        /// <summary>
        /// Name of the collection
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// description of the collection
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlElement(ElementName = "description", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaMultilingualString Description { get; set; }


        /// <summary>
        /// Collection usage module
        /// This property will deprecated soon. Please use usageModuleId instead of it.
        /// </summary>
        [DataMember(Name = "usageModule")]
        [JsonProperty("usageModule")]
        [XmlElement(ElementName = "usageModule", IsNullable = true)]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public KalturaUsageModule UsageModule { get; set; }

        /// <summary>
        /// The internal usage module identifier for the collection
        /// </summary>
        [DataMember(Name = "usageModuleId")]
        [JsonProperty("usageModuleId")]
        [XmlElement(ElementName = "usageModuleId", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLong = 1)]
        public long? UsageModuleId { get; set; }

        /// <summary>
        /// List of Coupons group
        /// This property will deprecated soon. Please use CollectionCouponGroup instead of it.
        /// </summary>
        [DataMember(Name = "couponsGroups")]
        [JsonProperty("couponsGroups")]
        [XmlArray(ElementName = "couponsGroups", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public List<KalturaCouponsGroup> CouponGroups { get; set; }

        /// <summary>
        /// List of collection Coupons group
        /// </summary>
        [DataMember(Name = "collectionCouponGroup")]
        [JsonProperty("collectionCouponGroup")]
        [XmlArray(ElementName = "collectionCouponGroup", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(IsNullable = true)]
        public List<KalturaCollectionCouponGroup> CollectionCouponGroup { get; set; }

        /// <summary>
        /// External ID
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        [SchemeProperty(IsNullable = true)]
        public string ExternalId { get; set; }

        /// <summary>
        /// List of Collection product codes
        /// </summary>
        [DataMember(Name = "productCodes")]
        [JsonProperty("productCodes")]
        [XmlArray(ElementName = "productCodes", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(IsNullable = true)]
        public List<KalturaProductCode> ProductCodes { get; set; }

        /// <summary>
        /// The ID of the price details associated with this collection
        /// </summary>
        [DataMember(Name = "priceDetailsId")]
        [JsonProperty("priceDetailsId")]
        [XmlElement(ElementName = "priceDetailsId")]
        [SchemeProperty(MinLong = 1)]
        public long? PriceDetailsId { get; set; }

        /// <summary>
        /// Is active collection
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Specifies when was the collection created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the collection last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// Virtual asset id
        /// </summary>
        [DataMember(Name = "virtualAssetId")]
        [JsonProperty("virtualAssetId")]
        [XmlElement(ElementName = "virtualAssetId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? VirtualAssetId { get; set; }

        /// <summary>
        /// A list of file types identifiers that are supported in this collection
        /// </summary>
        [DataMember(Name = "fileTypes")]
        [JsonProperty("fileTypes")]
        [XmlArray(ElementName = "fileTypes", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(IsNullable = true, ReadOnly = true)]
        public List<KalturaIntegerValue> FileTypes { get; set; }

        /// <summary>
        /// Comma separated file types identifiers that are supported in this collection
        /// </summary>
        [DataMember(Name = "fileTypesIds")]
        [JsonProperty("fileTypesIds")]
        [XmlArray(ElementName = "fileTypesIds", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string FileTypesIds { get; set; }

        /// <summary>
        /// Asset user rule identifier 
        /// </summary>
        [DataMember(Name = "assetUserRuleId")]
        [JsonProperty("assetUserRuleId")]
        [XmlElement(ElementName = "assetUserRuleId")]
        [SchemeProperty(RequiresPermission = (int)RequestType.INSERT, IsNullable = true)]
        public long? AssetUserRuleId { get; set; }

    }
}