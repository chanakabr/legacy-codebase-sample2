using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Program-asset info
    /// </summary>
    [Serializable]
    public partial class KalturaProgramAsset : KalturaAsset
    {
        /// <summary>
        /// EPG channel identifier
        /// </summary>
        [DataMember(Name = "epgChannelId")]
        [JsonProperty(PropertyName = "epgChannelId")]
        [XmlElement(ElementName = "epgChannelId")]
        [SchemeProperty(ReadOnly = true)]
        public long? EpgChannelId { get; set; }

        /// <summary>
        /// EPG identifier
        /// </summary>
        [DataMember(Name = "epgId")]
        [JsonProperty(PropertyName = "epgId")]
        [XmlElement(ElementName = "epgId")]
        [SchemeProperty(ReadOnly = true)]
        public string EpgId { get; set; }

        /// <summary>
        /// Ralated media identifier
        /// </summary>
        [DataMember(Name = "relatedMediaId")]
        [JsonProperty(PropertyName = "relatedMediaId")]
        [XmlElement(ElementName = "relatedMediaId")]
        public long? RelatedMediaId { get; set; }

        /// <summary>
        /// Unique identifier for the program
        /// </summary>
        [DataMember(Name = "crid")]
        [JsonProperty(PropertyName = "crid")]
        [XmlElement(ElementName = "crid")]
        public string Crid { get; set; }

        /// <summary>
        /// Id of linear media asset
        /// </summary>
        [DataMember(Name = "linearAssetId")]
        [JsonProperty(PropertyName = "linearAssetId")]
        [XmlElement(ElementName = "linearAssetId", IsNullable = true)]
        [SchemeProperty(InsertOnly = true)]
        public long? LinearAssetId { get; set; }

        /// <summary>
        /// Is CDVR enabled for this asset
        /// Please, note that value of this property is strictly connected with CDV-R setting on Partner and KalturaLiveAsset levels.
        /// In order to enable CDV-R for KalturaProgramAsset, Partner and KalturaLiveAsset CDV-R settings should be enabled.
        /// </summary>
        [DataMember(Name = "enableCdvr")]
        [JsonProperty(PropertyName = "enableCdvr")]
        [XmlElement(ElementName = "enableCdvr")]
        [OnlyNewStandard("5.0.0.0")]
        public bool? CdvrEnabled { get; set; }

        /// <summary>
        /// Is catch-up enabled for this asset
        /// Please, note that value of this property is strictly connected with Catch Up setting on Partner and KalturaLiveAsset levels.
        /// In order to enable Catch Up for KalturaProgramAsset, Partner and KalturaLiveAsset Catch Up settings should be enabled.
        /// </summary>
        [DataMember(Name = "enableCatchUp")]
        [JsonProperty(PropertyName = "enableCatchUp")]
        [XmlElement(ElementName = "enableCatchUp")]
        [OnlyNewStandard("5.0.0.0")]
        public bool? CatchUpEnabled { get; set; }

        /// <summary>
        /// Is start over enabled for this asset
        /// Please, note that value of this property is strictly connected with Start Over setting on Partner and KalturaLiveAsset levels.
        /// In order to enable Start Over for KalturaProgramAsset, Partner and KalturaLiveAsset Start Over settings should be enabled.
        /// </summary>
        [DataMember(Name = "enableStartOver")]
        [JsonProperty(PropertyName = "enableStartOver")]
        [XmlElement(ElementName = "enableStartOver")]
        [OnlyNewStandard("5.0.0.0")]
        public bool? StartOverEnabled { get; set; }

        /// <summary>
        /// Is trick-play enabled for this asset
        /// Please, note that value of this property is strictly connected with Trick Play setting on Partner and KalturaLiveAsset levels.
        /// In order to enable Trick Play for KalturaProgramAsset, Partner and KalturaLiveAsset Trick Play settings should be enabled.
        /// </summary>
        [DataMember(Name = "enableTrickPlay")]
        [JsonProperty(PropertyName = "enableTrickPlay")]
        [XmlElement(ElementName = "enableTrickPlay")]
        [OnlyNewStandard("5.0.0.0")]
        public bool? TrickPlayEnabled { get; set; }

        /// <summary>
        /// Contains comma separate list of KalturaProgramAssetGroupOffer.externalOfferId values indicating the PAGOs to which the Program Asset is bound.
        /// </summary>
        [DataMember(Name = "externalOfferIds")]
        [JsonProperty(PropertyName = "externalOfferIds")]
        [XmlElement(ElementName = "externalOfferIds")]
        public string ExternalOfferIds { get; set; }

        internal override void ValidateForInsert()
        {
            base.ValidateForInsert();

            if (!LinearAssetId.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "linearAssetId");
            }

            if (string.IsNullOrEmpty(Crid))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "crid");
            }
        }
    }
}