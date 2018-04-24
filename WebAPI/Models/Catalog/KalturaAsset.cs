using Jil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset info
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(KalturaRecordingAsset))]
    [XmlInclude(typeof(KalturaProgramAsset))]
    [XmlInclude(typeof(KalturaMediaAsset))]
    abstract public class KalturaAsset : KalturaOTTObject, KalturaIAssetable
    {

        private const string GENESIS_VERSION = "4.6.0.0";

        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }

        /// <summary>
        /// Identifies the asset type (EPG, Recording, Movie, TV Series, etc). 
        /// Possible values: 0 – EPG linear programs, 1 - Recording; or any asset type ID according to the asset types IDs defined in the system.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty(InsertOnly = true)]
        public int? Type { get; set; }

        /// <summary>
        /// Asset name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// Asset description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        [XmlElement(ElementName = "description")]
        public KalturaMultilingualString Description { get; set; }

        /// <summary>
        /// Collection of images details that can be used to represent this asset
        /// </summary>
        [DataMember(Name = "images", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "images", NullValueHandling = NullValueHandling.Ignore)]
        [XmlArray(ElementName = "images", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaMediaImage> Images { get; set; }

        /// <summary>
        /// Files
        /// </summary>
        [DataMember(Name = "mediaFiles", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "mediaFiles", NullValueHandling = NullValueHandling.Ignore)]
        [XmlArray(ElementName = "mediaFiles", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(ReadOnly = true)]
        public List<KalturaMediaFile> MediaFiles { get; set; }

        /// <summary>
        /// Collection of add-on statistical information for the media. See AssetStats model for more information
        /// </summary>
        [DataMember(Name = "stats", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "stats", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement(ElementName = "stats", IsNullable = true)]
        [JilDirectiveAttribute(Ignore = true)]
        [Obsolete]
        public KalturaAssetStatistics Statistics { get; set; }

        /// <summary>
        /// Dynamic collection of key-value pairs according to the String Meta defined in the system
        /// </summary>
        [DataMember(Name = "metas")]
        [JsonProperty(PropertyName = "metas")]
        [XmlElement("metas", IsNullable = true)]
        public SerializableDictionary<string, KalturaValue> Metas { get; set; }

        /// <summary>
        /// Dynamic collection of key-value pairs according to the Tag Types defined in the system
        /// </summary>
        [DataMember(Name = "tags")]
        [JsonProperty(PropertyName = "tags")]
        [XmlElement("tags", IsNullable = true)]
        public SerializableDictionary<string, KalturaMultilingualStringValueArray> Tags { get; set; }

        /// <summary>
        /// Date and time represented as epoch. For VOD – since when the asset is available in the catalog. For EPG/Linear – when the program is aired (can be in the future).
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty(PropertyName = "startDate")]
        [XmlElement(ElementName = "startDate", IsNullable = true)]
        public long? StartDate { get; set; }

        /// <summary>
        /// Date and time represented as epoch. For VOD – till when the asset be available in the catalog. For EPG/Linear – program end time and date
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty(PropertyName = "endDate")]
        [XmlElement(ElementName = "endDate", IsNullable = true)]
        public long? EndDate { get; set; }

        /// <summary>
        /// Specifies when was the Asset was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the Asset last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// Enable cDVR
        /// </summary>
        [DataMember(Name = "enableCdvr")]
        [JsonProperty(PropertyName = "enableCdvr")]
        [XmlElement(ElementName = "enableCdvr")]
        [Deprecated(GENESIS_VERSION)]
        public bool? EnableCdvr { get; set; }

        /// <summary>
        /// Enable catch-up
        /// </summary>
        [DataMember(Name = "enableCatchUp")]
        [JsonProperty(PropertyName = "enableCatchUp")]
        [XmlElement(ElementName = "enableCatchUp")]
        [Deprecated(GENESIS_VERSION)]
        public bool? EnableCatchUp { get; set; }

        /// <summary>
        /// Enable start over
        /// </summary>
        [DataMember(Name = "enableStartOver")]
        [JsonProperty(PropertyName = "enableStartOver")]
        [XmlElement(ElementName = "enableStartOver")]
        [Deprecated(GENESIS_VERSION)]
        public bool? EnableStartOver { get; set; }

        /// <summary>
        /// Enable trick-play
        /// </summary>
        [DataMember(Name = "enableTrickPlay")]
        [JsonProperty(PropertyName = "enableTrickPlay")]
        [XmlElement(ElementName = "enableTrickPlay")]
        [Deprecated(GENESIS_VERSION)]
        public bool? EnableTrickPlay { get; set; }

        /// <summary>
        /// External identifier for the asset
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty(PropertyName = "externalId")]
        [XmlElement(ElementName = "externalId")]        
        public string ExternalId { get; set; }

        internal int getType()
        {
            return Type.HasValue ? (int)Type : 0;
        }

        internal void ValidateTags()
        {
            if (Tags != null && Tags.Count > 0)
            {
                foreach (KeyValuePair<string, KalturaMultilingualStringValueArray> tagValues in Tags)
                {                    
                    if (tagValues.Value.Objects != null && tagValues.Value.Objects.Count > 0)
                    {
                        foreach (KalturaMultilingualStringValue item in tagValues.Value.Objects)
                        {
                            List<ApiObjects.LanguageContainer> noneDefaultLanugageContainer = item.value.GetNoneDefaultLanugageContainer();
                            if (noneDefaultLanugageContainer != null && noneDefaultLanugageContainer.Count > 0)
                            {
                                throw new BadRequestException(ApiException.TAG_TRANSLATION_NOT_ALLOWED);
                            }
                        }
                    }

                }
            }
        }

        internal void ValidateMetas()
        {
            if (Metas != null && Metas.Count > 0)
            {
                foreach (KeyValuePair<string, KalturaValue> metaValues in Metas)
                {
                    if (metaValues.Value.GetType() == typeof(KalturaMultilingualStringValue))
                    {
                        KalturaMultilingualStringValue multilingualStringValue = metaValues.Value as KalturaMultilingualStringValue;
                        if (multilingualStringValue != null)
                        {
                            multilingualStringValue.value.Validate(metaValues.Key);
                        }
                    }
                }
            }            
        }
    }
}