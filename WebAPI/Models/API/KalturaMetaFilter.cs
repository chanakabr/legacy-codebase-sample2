using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Meta filter
    /// </summary>
    public partial class KalturaMetaFilter : KalturaFilter<KalturaMetaOrderBy>
    {
        /// <summary>
        /// Meta system field name to filter by
        /// </summary>
        [DataMember(Name = "fieldNameEqual")]
        [JsonProperty("fieldNameEqual")]
        [XmlElement(ElementName = "fieldNameEqual")]
        public KalturaMetaFieldName? FieldNameEqual { get; set; }

        /// <summary>
        /// Meta system field name to filter by
        /// </summary>
        [DataMember(Name = "fieldNameNotEqual")]
        [JsonProperty("fieldNameNotEqual")]
        [XmlElement(ElementName = "fieldNameNotEqual")]
        public KalturaMetaFieldName? FieldNameNotEqual { get; set; }

        /// <summary>
        /// Meta type to filter by
        /// </summary>
        [DataMember(Name = "typeEqual")]
        [JsonProperty("typeEqual")]
        [XmlElement(ElementName = "typeEqual")]
        public KalturaMetaType? TypeEqual { get; set; }

        /// <summary>
        /// Asset type to filter by
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty("assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual")]
        public KalturaAssetType? AssetTypeEqual { get; set; }

        /// <summary>
        /// Features
        /// </summary>
        [DataMember(Name = "featuresIn")]
        [JsonProperty("featuresIn")]
        [XmlElement(ElementName = "featuresIn", IsNullable = true)]
        public string FeaturesIn { get; set; }

        public override KalturaMetaOrderBy GetDefaultOrderByValue()
        {
            return KalturaMetaOrderBy.NONE;
        }

        public List<KalturaMetaFeatureType> GetFeaturesIn()
        {
            List<KalturaMetaFeatureType> featureList = new List<KalturaMetaFeatureType>();
            if (!string.IsNullOrEmpty(FeaturesIn))
            {
                featureList = new List<KalturaMetaFeatureType>();
                string[] metaFeatures = FeaturesIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string feature in metaFeatures)
                {
                    KalturaMetaFeatureType kalturaMetaFeatureType;
                    if (Enum.TryParse<KalturaMetaFeatureType>(feature.ToUpper(), out kalturaMetaFeatureType))
                    {
                        featureList.Add(kalturaMetaFeatureType);
                    }
                }
            }

            return featureList;
        }

        internal void validate()
        {
            if (FieldNameNotEqual.HasValue && FieldNameEqual.HasValue)
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaMetaFilter.fieldNameEqual", "KalturaMetaFilter.fieldNameNotEqual");

            if (TypeEqual.HasValue && (TypeEqual.Value ==KalturaMetaType.NUMBER || TypeEqual.Value ==KalturaMetaType.BOOLEAN) && 
                (AssetTypeEqual == KalturaAssetType.recording || AssetTypeEqual == KalturaAssetType.epg))
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaMetaFilter.typeEqual", "KalturaMetaFilter.assetTypeEqual");

            if (FieldNameNotEqual.HasValue && AssetTypeEqual == KalturaAssetType.media)
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaMetaFilter.fieldNameNotEqual", "KalturaMetaFilter.assetTypeEqual");

            if (FieldNameEqual.HasValue && FieldNameEqual.Value != KalturaMetaFieldName.NONE && AssetTypeEqual == KalturaAssetType.media)
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaMetaFilter.fieldNameEqual", "KalturaMetaFilter.assetTypeEqual");
        }
    }
}