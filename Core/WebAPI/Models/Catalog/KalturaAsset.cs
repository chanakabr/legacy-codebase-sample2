using Jil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset info
    /// </summary>
    [Serializable]
    [XmlInclude(typeof(KalturaRecordingAsset))]
    [XmlInclude(typeof(KalturaProgramAsset))]
    [XmlInclude(typeof(KalturaMediaAsset))]
    abstract public partial class KalturaAsset : KalturaOTTObject, KalturaIAssetable
    {
        #region Consts

        private const string OPC_MERGE_VERSION = "5.0.0.0";

        #endregion

        #region Data Members

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
        [SchemeProperty(IsNullable = true)]
        public SerializableDictionary<string, KalturaValue> Metas { get; set; }

        /// <summary>
        /// Dynamic collection of key-value pairs according to the Tag Types defined in the system
        /// </summary>
        [DataMember(Name = "tags")]
        [JsonProperty(PropertyName = "tags")]
        [XmlElement("tags", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public SerializableDictionary<string, KalturaMultilingualStringValueArray> Tags { get; set; }

        /// <summary>
        /// Dynamic collection of key-value pairs according to the related entity defined in the system
        /// </summary>
        [DataMember(Name = "relatedEntities")]
        [JsonProperty(PropertyName = "relatedEntities")]
        [XmlElement("relatedEntities", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public SerializableDictionary<string, KalturaRelatedEntityArray> RelatedEntities { get; set; }

        /// <summary>
        /// Date and time represented as epoch. For VOD – since when the asset is available in the catalog. For EPG/Linear – when the program is aired (can be in the future).
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty(PropertyName = "startDate")]
        [XmlElement(ElementName = "startDate", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? StartDate { get; set; }

        /// <summary>
        /// Date and time represented as epoch. For VOD – till when the asset be available in the catalog. For EPG/Linear – program end time and date
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty(PropertyName = "endDate")]
        [XmlElement(ElementName = "endDate", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
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
        [Deprecated(OPC_MERGE_VERSION)]
        public bool? EnableCdvr { get; set; }

        /// <summary>
        /// Enable catch-up
        /// </summary>
        [DataMember(Name = "enableCatchUp")]
        [JsonProperty(PropertyName = "enableCatchUp")]
        [XmlElement(ElementName = "enableCatchUp")]
        [Deprecated(OPC_MERGE_VERSION)]
        public bool? EnableCatchUp { get; set; }

        /// <summary>
        /// Enable start over
        /// </summary>
        [DataMember(Name = "enableStartOver")]
        [JsonProperty(PropertyName = "enableStartOver")]
        [XmlElement(ElementName = "enableStartOver")]
        [Deprecated(OPC_MERGE_VERSION)]
        public bool? EnableStartOver { get; set; }

        /// <summary>
        /// Enable trick-play
        /// </summary>
        [DataMember(Name = "enableTrickPlay")]
        [JsonProperty(PropertyName = "enableTrickPlay")]
        [XmlElement(ElementName = "enableTrickPlay")]
        [Deprecated(OPC_MERGE_VERSION)]
        public bool? EnableTrickPlay { get; set; }

        /// <summary>
        /// External identifier for the asset
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty(PropertyName = "externalId")]
        [XmlElement(ElementName = "externalId")]
        public string ExternalId { get; set; }

        /// <summary>
        ///  The media asset index status
        /// </summary>
        [DataMember(Name = "indexStatus")]
        [JsonProperty("indexStatus")]
        [XmlElement(ElementName = "indexStatus", IsNullable = true)]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ, ReadOnly = true)]        
        public KalturaAssetIndexStatus? IndexStatus { get; set; }

        #endregion

        internal int getType()
        {
            return Type.HasValue ? Type.Value : 0;
        }

        internal virtual void ValidateForInsert()
        {
            if (!(this is KalturaLiveAsset) && !(this is KalturaMediaAsset) && !(this is KalturaProgramAsset))
            {
                throw new ClientException((int)StatusCode.Error, "Invalid assetType");
            }

            if ((this is KalturaProgramAsset) && this.Type.HasValue && this.Type.Value != 0)
            {
                throw new ClientException((int)StatusCode.Error, "Invalid type value");
            }

            if (this.Name == null || this.Name.Values == null || this.Name.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }
            this.Name.Validate("multilingualName");

            if (this.Description != null && this.Description.Values != null && this.Description.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
            }

            if (this.Description != null)
            {
                this.Description.Validate("multilingualDescription");
            }

            if (!this.Type.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "type");
            }

            if (string.IsNullOrEmpty(this.ExternalId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalId");
            }
            
            this.ValidateMetas();
            this.ValidateTags();
            this.ValidateRelatedEntities();
        }

        internal virtual void ValidateForUpdate()
        {
            if (!(this is KalturaLiveAsset) && !(this is KalturaMediaAsset) && !(this is KalturaProgramAsset))
            {
                throw new ClientException((int)StatusCode.Error, "Invalid assetType");
            }

            if (this.Name != null)
            {
                if ((this.Name.Values == null || this.Name.Values.Count == 0))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }
                else
                {
                    this.Name.Validate("multilingualName");
                }
            }

            if (this.Description != null)
            {
                if ((this.Description.Values == null || this.Description.Values.Count == 0))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
                }
                else
                {
                    this.Description.Validate("multilingualDescription", true, false);
                }
            }

            if (this.ExternalId != null && this.ExternalId == string.Empty)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalId");
            }
            
            this.ValidateMetas();
            this.ValidateTags();
            this.ValidateRelatedEntities();
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
                            if (item.value == null)
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, $"KalturaMultilingualStringValue.value {tagValues.Key}");
                            }

                            List<ApiObjects.LanguageContainer> noneDefaultLanugageContainer = item.value.GetNoneDefaultLanugageContainer();
                            if (noneDefaultLanugageContainer != null && noneDefaultLanugageContainer.Count > 0)
                            {
                                throw new BadRequestException(BadRequestException.TAG_TRANSLATION_NOT_ALLOWED);
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
                    if (metaValues.Value is KalturaMultilingualStringValue)
                    {
                        KalturaMultilingualStringValue multilingualStringValue = metaValues.Value as KalturaMultilingualStringValue;
                        if (multilingualStringValue.value == null)
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaMultilingualStringValue.value");
                        }

                        multilingualStringValue.value.Validate(metaValues.Key);
                    }
                }
            }
        }

        internal void ValidateRelatedEntities()
        {
            if(RelatedEntities?.Count > 5)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED, "asset.relatedEntities", 5);
            }

            if (RelatedEntities?.Count > 0)
            {
                foreach (KeyValuePair<string, KalturaRelatedEntityArray> relatedEntityArray in RelatedEntities)
                {
                    if (relatedEntityArray.Value?.Objects?.Count > 0)
                    {
                        if (relatedEntityArray.Value?.Objects?.Count > 20)
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED, "asset.relatedEntities.objects", 20);

                        }

                        foreach (KalturaRelatedEntity item in relatedEntityArray.Value.Objects)
                        {
                            if (item == null)
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "item");
                            }                          
                        }
                    }
                }
            }
        }
    }
}