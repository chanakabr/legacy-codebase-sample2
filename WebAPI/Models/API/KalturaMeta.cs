using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Filters;
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

        private const string FEATURES_PATTERN = @"\W";

        /// <summary>
        /// Meta id 
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public string Id { get; set; }

        /// <summary>
        /// Meta name for the partner
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name", IsNullable = true)]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// Meta system name for the partner
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty("systemName")]
        [XmlElement(ElementName = "systemName", IsNullable = true)]
        public string SystemName { get; set; }

        /// <summary>
        /// Meta system field name 
        /// </summary>
        [DataMember(Name = "fieldName")]
        [JsonProperty("fieldName")]
        [XmlElement(ElementName = "fieldName")]
        [Deprecated("4.6.0.0")]
        public KalturaMetaFieldName FieldName { get; set; }

        /// <summary>
        ///  Meta value type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type", IsNullable = true)]
        public KalturaMetaType Type { get; set; }

        /// <summary>
        ///  Does the meta contain multiple values
        /// </summary>
        [DataMember(Name = "multipleValue")]
        [JsonProperty("multipleValue")]
        [XmlElement(ElementName = "multipleValue", IsNullable = true)]
        public bool MultipleValue { get; set; }

        /// <summary>
        ///  Is the meta protected by the system
        /// </summary>
        [DataMember(Name = "isProtected")]
        [JsonProperty("isProtected")]
        [XmlElement(ElementName = "isProtected", IsNullable = true)]
        public bool IsProtected { get; set; }

        /// <summary>
        ///  The help text of the meta to display on the UI, where needed.
        /// </summary>
        [DataMember(Name = "helpText")]
        [JsonProperty("helpText")]
        [XmlElement(ElementName = "helpText", IsNullable = true)]        
        public string HelpText { get; set; }

        /// <summary>
        /// Asset type this meta is related to 
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty("assetType")]
        [XmlElement(ElementName = "assetType")]
        [Deprecated("4.6.0.0")]
        public KalturaAssetType AssetType { get; set; }

        /// <summary>
        /// List of supported features 
        /// </summary>
        [DataMember(Name = "features")]
        [JsonProperty("features")]
        [XmlElement(ElementName = "features", IsNullable = true)]
        //[SchemeProperty(DynamicType = typeof(KalturaMetaFeatureType))]
        public string Features { get; set; }

        /// <summary>
        /// Parent meta id
        /// </summary>
        [DataMember(Name = "parentId")]
        [JsonProperty("parentId")]
        [XmlElement(ElementName = "parentId", IsNullable = true)]        
        public string ParentId{ get; set; }

        /// <summary>
        /// Partner Id
        /// </summary>
        [DataMember(Name = "partnerId")]
        [JsonProperty("partnerId")]
        [XmlElement(ElementName = "partnerId")]
        [Deprecated("4.6.0.0")]
        public int PartnerId { get; set; }

        /// <summary>
        /// Specifies when was the meta was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the meta last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        internal void Validate()
        {
            if (MultipleValue && Type != KalturaMetaType.STRING)
            {
                throw new BadRequestException(ApiException.INVALID_MULTIPLE_VALUE_FOR_META_TYPE);
            }

            if (!string.IsNullOrEmpty(this.Features))
            {
                string allowedPattern = TCMClient.Settings.Instance.GetValue<string>("meta_features_patten");
                if (string.IsNullOrEmpty(allowedPattern))
                {
                    allowedPattern = FEATURES_PATTERN;
                }

                Regex regex = new Regex(allowedPattern);
                if (regex.IsMatch(this.Features))
                {
                    throw new BadRequestException(ApiException.INVALID_VALUE_FOR_FEATURES);
                }                               
            }
        }

        internal HashSet<string> GetFeaturesAsHashSet()
        {
            HashSet<string> result = new HashSet<string>();
            if (!string.IsNullOrEmpty(this.Features))
            {
                string[] splitedFeatures = this.Features.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string feature in splitedFeatures)
                {
                    if (!result.Contains(feature))
                    {
                        result.Add(feature);
                    }
                }
            }

            return result;
        }

        public string GetCommaSeparatedFeatures()
        {
            if (this.Features != null && this.Features.Length > 0)
            {
                return string.Join(",", this.Features);
            }
            else
            {
                return string.Empty;
            }
        }
    }

    [Serializable]
    public enum KalturaMetaFeatureType
    {
        USER_INTEREST,
        ENABLED_NOTIFICATION,
        SEARCH_RELATED,
        NOT_EDITABLE,
        VALUE_REQUIRED
    } 
}