using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner catalog configuration
    /// </summary>
    public partial class KalturaCatalogPartnerConfig : KalturaPartnerConfiguration
    {
        protected override KalturaPartnerConfigurationType ConfigurationType { get { return KalturaPartnerConfigurationType.Catalog; } }

        /// <summary>
        /// Single multilingual mode
        /// </summary>
        [DataMember(Name = "singleMultilingualMode")]
        [JsonProperty("singleMultilingualMode")]
        [XmlElement(ElementName = "singleMultilingualMode")]
        [SchemeProperty(IsNullable = true)]
        public bool? SingleMultilingualMode { get; set; }

        /// <summary>
        /// Category management
        /// </summary>
        [DataMember(Name = "categoryManagement")]
        [JsonProperty("categoryManagement")]
        [XmlElement(ElementName = "categoryManagement", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaCategoryManagement CategoryManagement { get; set; }
        
        /// <summary>
        /// EPG Multilingual Fallback Support
        /// </summary>
        [DataMember(Name = "epgMultilingualFallbackSupport")]
        [JsonProperty("epgMultilingualFallbackSupport")]
        [XmlElement(ElementName = "epgMultilingualFallbackSupport")]
        [SchemeProperty(IsNullable = true)]
        public bool? EpgMultilingualFallbackSupport { get; set; }

        /// <summary>
        /// Upload Export Datalake
        /// </summary>
        [DataMember(Name = "uploadExportDatalake")]
        [JsonProperty("uploadExportDatalake")]
        [XmlElement(ElementName = "uploadExportDatalake")]
        [SchemeProperty(IsNullable = true)]
        public bool? UploadExportDatalake { get; set; }

        internal override bool Update(int groupId)
        {
            Func<CatalogPartnerConfig, Status> partnerConfigFunc =
                (CatalogPartnerConfig catalogPartnerConfig) => CatalogPartnerConfigManager.Instance.UpdateCatalogConfig(groupId, catalogPartnerConfig);

            ClientUtils.GetResponseStatusFromWS(partnerConfigFunc, this);

            return true;
        }

        public override void ValidateForUpdate()
        {
            if (this.CategoryManagement != null)
            {
                this.CategoryManagement.ValidateForUpdate();
            }
        }
    }

    /// <summary>
    /// Category management
    /// </summary>
    public partial class KalturaCategoryManagement : KalturaOTTObject
    {
        /// <summary>
        /// Default CategoryVersion tree id
        /// </summary>
        [DataMember(Name = "defaultTreeId")]
        [JsonProperty("defaultTreeId")]
        [XmlElement(ElementName = "defaultTreeId")]
        [SchemeProperty(IsNullable = true)]
        public long? DefaultCategoryTreeId { get; set; }

        /// <summary>
        /// Device family to Category TreeId mapping
        /// </summary>
        [DataMember(Name = "deviceFamilyToCategoryTree")]
        [JsonProperty("deviceFamilyToCategoryTree")]
        [XmlElement(ElementName = "deviceFamilyToCategoryTree", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public SerializableDictionary<string, KalturaLongValue> DeviceFamilyToCategoryTree { get; set; }

        internal void ValidateForUpdate()
        {
            if (DeviceFamilyToCategoryTree != null && !DefaultCategoryTreeId.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "defaultTreeId");
            }
        }
    }
}