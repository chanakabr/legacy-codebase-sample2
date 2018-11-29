using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
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
        /// </summary>
        [DataMember(Name = "enableCdvr")]
        [JsonProperty(PropertyName = "enableCdvr")]
        [XmlElement(ElementName = "enableCdvr")]        
        public bool? CdvrEnabled { get; set; }

        /// <summary>
        /// Is catch-up enabled for this asset
        /// </summary>
        [DataMember(Name = "enableCatchUp")]
        [JsonProperty(PropertyName = "enableCatchUp")]
        [XmlElement(ElementName = "enableCatchUp")]        
        public bool? CatchUpEnabled { get; set; }

        /// <summary>
        /// Is start over enabled for this asset
        /// </summary>
        [DataMember(Name = "enableStartOver")]
        [JsonProperty(PropertyName = "enableStartOver")]
        [XmlElement(ElementName = "enableStartOver")]        
        public bool? StartOverEnabled { get; set; }

        /// <summary>
        /// Is trick-play enabled for this asset
        /// </summary>
        [DataMember(Name = "enableTrickPlay")]
        [JsonProperty(PropertyName = "enableTrickPlay")]
        [XmlElement(ElementName = "enableTrickPlay")]        
        public bool? TrickPlayEnabled { get; set; }

        internal override void ValidateForInsert()
        {
            base.ValidateForInsert();

            if (!EpgChannelId.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "epgChannelId");
            }

            if (string.IsNullOrEmpty(Crid))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "crid");
            }
        }
    }
}