using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Program asset group offer details
    /// </summary>
    public partial class KalturaProgramAssetGroupOffer : KalturaOTTObjectSupportNullable
    {
        /// <summary>
        /// Unique Kaltura internal identifier for the module
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Name of the Program asset group offer
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public KalturaMultilingualString Name { get; set; }


        /// <summary>
        /// ID of the KalturaPriceDetails object which contains details of the price to be paid for purchasing this KalturaProgramAssetGroupOffer.
        /// </summary>
        [DataMember(Name = "priceDetailsId")]
        [JsonProperty("priceDetailsId")]
        [XmlElement(ElementName = "priceDetailsId", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? PriceDetailsId { get; set; }

        /// <summary>
        /// Comma separated file types identifiers that are supported in this Program asset group offer.
        /// The subset of KalturaMediaFiles of the live linear channel on which the associated Program Assets are carried to which households entitled to this
        /// Program Asset Group Offer are entitled to view E.g.may be used to restrict entitlement only to HD flavour of the Program Asset(and not the UHD flavour)
        /// If this parameter is empty, the Household shall be entitled to all KalturaMediaFiles associated with the KalturaLiveAsset. 
        /// </summary>
        [DataMember(Name = "fileTypesIds")]
        [JsonProperty("fileTypesIds")]
        [XmlArray(ElementName = "fileTypesIds", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string FileTypesIds { get; set; }

        /// <summary>
        /// A list of the descriptions of the Program asset group offer on different languages (language code and translation)
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty("description")]
        [XmlArray(ElementName = "description", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(IsNullable = true)]
        public KalturaMultilingualString Description { get; set; }

        /// <summary>
        /// The id of the paired asset
        /// </summary>
        [DataMember(Name = "virtualAssetId")]
        [JsonProperty("virtualAssetId")]
        [XmlElement(ElementName = "virtualAssetId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? VirtualAssetId { get; set; }

        /// <summary>
        /// Indicates whether the PAGO is active or not (includes whether the PAGO can be purchased and whether it is returned in list API response for regular users)
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive", IsNullable = true)]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Specifies when was the pago created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the pago last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long? UpdateDate { get; set; }

        /// <summary>
        /// The date/time at which the Program Asset Group Offer is first purchasable by households. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty(PropertyName = "startDate")]
        [XmlElement(ElementName = "startDate")]
        [SchemeProperty(IsNullable = true)]
        public long? StartDate { get; set; }

        /// <summary>
        /// The date/time at which the Program Asset Group Offer is last purchasable by households.Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty(PropertyName = "endDate")]
        [XmlElement(ElementName = "endDate")]
        [SchemeProperty(IsNullable = true)]
        public long? EndDate { get; set; }

        /// <summary>
        /// The last date/time at which the system will attempt to locate Program Assets that may be associated with this offer.Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "expiryDate")]
        [JsonProperty(PropertyName = "expiryDate")]
        [XmlElement(ElementName = "expiryDate")]
        [SchemeProperty(IsNullable = true)]
        public long? ExpiryDate { get; set; }

        /// <summary>
        /// External identifier
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        [SchemeProperty(IsNullable = true)]
        public string ExternalId { get; set; }

        /// <summary>
        /// Identifies the Program Assets which will be entitled by Households that purchase this offer. Must be a unique value in the context of an account. 
        /// </summary>
        [DataMember(Name = "externalOfferId")]
        [JsonProperty("externalOfferId")]
        [XmlElement(ElementName = "externalOfferId")]
        [SchemeProperty(IsNullable = true)]
        public string ExternalOfferId { get; set; }
    }
}