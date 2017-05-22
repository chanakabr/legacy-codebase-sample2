using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Asset meta
    /// </summary>
    public class KalturaMeta : KalturaOTTObject
    {
        /// <summary>
        /// Meta name for the partner
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Meta system field name 
        /// </summary>
        [DataMember(Name = "fieldName")]
        [JsonProperty("fieldName")]
        [XmlElement(ElementName = "fieldName")]
        public KalturaMetaFieldName FieldName { get; set; }

        /// <summary>
        ///  Meta value type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public KalturaMetaType Type { get; set; }

        /// <summary>
        /// Asset type this meta is related to 
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty("assetType")]
        [XmlElement(ElementName = "assetType")]
        public KalturaAssetType AssetType { get; set; }

        /// <summary>
        /// List of supported features 
        /// </summary>
        [DataMember(Name = "features")]
        [JsonProperty("features")]
        [XmlElement(ElementName = "features", IsNullable = true)]
        [SchemeProperty(DynamicType = typeof(KalturaMetaFeatureType))]
        public string Features { get; set; }

        /// <summary>
        /// User interest default values 
        /// </summary>
        [DataMember(Name = "defaultValues")]
        [JsonProperty("defaultValues")]
        [XmlElement(ElementName = "defaultValues ")]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaStringValue> DefaultValues { get; set; }

        public List<KalturaMetaFeatureType> MetaFeatures()
        {
            List<KalturaMetaFeatureType> featureList = new List<KalturaMetaFeatureType>();
            if (!string.IsNullOrEmpty(this.Features))
            {
                string[] metaFeatures = this.Features.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string feature in metaFeatures)
                {
                    KalturaMetaFeatureType socialActionType;
                    if (Enum.TryParse<KalturaMetaFeatureType>(feature.ToUpper(), out socialActionType))
                    {
                        featureList.Add(socialActionType);
                    }
                }
            }
            else // fill in with all features
            {
                featureList.AddRange(Enum.GetValues(typeof(KalturaMetaFeatureType)).Cast<KalturaMetaFeatureType>().ToList());
            }

            return featureList;
        }
    }

    [Serializable]
    public enum KalturaMetaFeatureType
    {
        USER_INTEREST
    } 

}