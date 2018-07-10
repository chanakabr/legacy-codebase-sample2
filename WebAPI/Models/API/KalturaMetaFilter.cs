using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Meta filter
    /// </summary>
    public partial class KalturaMetaFilter : KalturaFilter<KalturaMetaOrderBy>
    {

        private const string OPC_MERGE_VERSION = "5.0.0.0";

        /// <summary>
        /// Comma separated identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        [SchemeProperty(DynamicMinInt = 1)]
        public string IdIn { get; set; }

        /// <summary>
        /// Filter Metas that are contained in a specific asset struct
        /// </summary>
        [DataMember(Name = "assetStructIdEqual")]
        [JsonProperty("assetStructIdEqual")]
        [XmlElement(ElementName = "assetStructIdEqual", IsNullable = true)]
        [SchemeProperty(MinLong = 1)]
        public long? AssetStructIdEqual { get; set; }

        /// <summary>
        /// Meta system field name to filter by
        /// </summary>
        [DataMember(Name = "fieldNameEqual")]
        [JsonProperty("fieldNameEqual")]
        [XmlElement(ElementName = "fieldNameEqual")]
        [Deprecated(OPC_MERGE_VERSION)]
        public KalturaMetaFieldName? FieldNameEqual { get; set; }

        /// <summary>
        /// Meta system field name to filter by
        /// </summary>
        [DataMember(Name = "fieldNameNotEqual")]
        [JsonProperty("fieldNameNotEqual")]
        [XmlElement(ElementName = "fieldNameNotEqual")]
        [Deprecated(OPC_MERGE_VERSION)]
        public KalturaMetaFieldName? FieldNameNotEqual { get; set; }

        /// <summary>
        /// Meta type to filter by
        /// </summary>
        [DataMember(Name = "typeEqual")]
        [JsonProperty("typeEqual")]
        [XmlElement(ElementName = "typeEqual")]
        [Deprecated(OPC_MERGE_VERSION)]
        public KalturaMetaType? TypeEqual { get; set; }

        /// <summary>
        /// Meta data type to filter by
        /// </summary>
        [DataMember(Name = "dataTypeEqual")]
        [JsonProperty("dataTypeEqual")]
        [XmlElement(ElementName = "dataTypeEqual", IsNullable = true)]
        public KalturaMetaDataType? DataTypeEqual { get; set; }

        /// <summary>
        /// Filter metas by multipleValueEqual value
        /// </summary>
        [DataMember(Name = "multipleValueEqual")]
        [JsonProperty("multipleValueEqual")]
        [XmlElement(ElementName = "multipleValueEqual", IsNullable = true)]        
        public bool? MultipleValueEqual { get; set; }

        /// <summary>
        /// Asset type to filter by
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty("assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual")]
        [Deprecated(OPC_MERGE_VERSION)]
        public KalturaAssetType? AssetTypeEqual { get; set; }

        /// <summary>
        /// Features
        /// </summary>
        [DataMember(Name = "featuresIn")]
        [JsonProperty("featuresIn")]
        [XmlElement(ElementName = "featuresIn", IsNullable = true)]
        [Deprecated(OPC_MERGE_VERSION)]
        public string FeaturesIn { get; set; }

        public override KalturaMetaOrderBy GetDefaultOrderByValue()
        {
            return KalturaMetaOrderBy.NAME_ASC;
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

        public List<long> GetIdIn()
        {
            HashSet<long> list = new HashSet<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && !list.Contains(value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaMetaFilter.idIn");
                    }
                }
            }

            return new List<long>(list);
        }

        internal void Validate()
        {
            if (!string.IsNullOrEmpty(IdIn) && AssetStructIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaMetaFilter.idIn", "KalturaMetaFilter.assetStructIdEqual");
            }

            if (DataTypeEqual.HasValue)
            {
                if (DataTypeEqual.Value != KalturaMetaDataType.STRING && MultipleValueEqual.HasValue && MultipleValueEqual.Value)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaMetaFilter.dataTypeEqual", "KalturaMetaFilter.multipleValueEqual");
                }
                else if (DataTypeEqual.Value == KalturaMetaDataType.STRING && !MultipleValueEqual.HasValue)
                {
                    throw new BadRequestException(ApiException.MULTI_VALUE_NOT_SENT_FOR_META_DATA_TYPE_STRING);
                }
            }
        }

        internal void OldValidate()
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