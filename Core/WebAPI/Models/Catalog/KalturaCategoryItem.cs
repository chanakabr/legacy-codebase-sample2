using ApiLogic.Base;
using ApiLogic.Catalog;
using ApiObjects.Base;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
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
    /// Category details
    /// </summary>
    [Serializable]
    public partial class KalturaCategoryItem : KalturaCrudObject<CategoryItem, long>
    {
        /// <summary>
        /// Unique identifier for the category
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// Category parent identifier 
        /// </summary>
        [DataMember(Name = "parentId")]
        [JsonProperty(PropertyName = "parentId")]
        [XmlElement(ElementName = "parentId")]
        [SchemeProperty(ReadOnly = true)]
        public long ParentId { get; set; }

        /// <summary>
        /// Comma separated list of child categories' Ids.
        /// </summary>
        [DataMember(Name = "childrenIds")]
        [JsonProperty(PropertyName = "childrenIds")]
        [XmlElement(ElementName = "childrenIds")]
        public string ChildrenIds { get; set; }

        /// <summary>
        /// List of unified Channels.
        /// </summary>
        [DataMember(Name = "unifiedChannels")]
        [JsonProperty(PropertyName = "unifiedChannels")]
        [XmlArray(ElementName = "unifiedChannels", IsNullable = true)]
        [XmlArrayItem("item")]
        [SchemeProperty(IsNullable = true)]
        public List<KalturaUnifiedChannel> UnifiedChannels { get; set; }

        /// <summary>
        /// Dynamic data
        /// </summary>
        [DataMember(Name = "dynamicData")]
        [JsonProperty("dynamicData")]
        [XmlElement(ElementName = "dynamicData", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> DynamicData { get; set; }

        /// <summary>
        /// Specifies when was the Category last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// Category active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        [SchemeProperty(IsNullable = true)]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Start date in seconds
        /// </summary>
        [DataMember(Name = "startDateInSeconds")]
        [JsonProperty("startDateInSeconds")]
        [XmlElement(ElementName = "startDateInSeconds", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? StartDateInSeconds { get; set; }

        /// <summary>
        /// End date in seconds
        /// </summary>
        [DataMember(Name = "endDateInSeconds")]
        [JsonProperty("endDateInSeconds")]
        [XmlElement(ElementName = "endDateInSeconds", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public long? EndDateInSeconds { get; set; }

        /// <summary>
        /// Category type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty(InsertOnly = true)]
        public string Type { get; set; }

        /// <summary>
        /// Unique identifier for the category version
        /// </summary>
        [DataMember(Name = "versionId")]
        [JsonProperty(PropertyName = "versionId")]
        [XmlElement(ElementName = "versionId")]
        [SchemeProperty(ReadOnly = true)]
        public long? VersionId { get; set; }

        /// <summary>
        /// Virtual asset id
        /// </summary>
        [DataMember(Name = "virtualAssetId")]
        [JsonProperty("virtualAssetId")]
        [XmlElement(ElementName = "virtualAssetId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? VirtualAssetId { get; set; }

        /// <summary>
        /// Category reference identifier
        /// </summary>
        [DataMember(Name = "referenceId")]
        [JsonProperty("referenceId")]
        [XmlElement(ElementName = "referenceId", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string ReferenceId { get; set; }

        internal override ICrudHandler<CategoryItem, long> Handler
        {
            get
            {
                return CategoryItemHandler.Instance;
            }
        }

        internal override void SetId(long id)
        {
            this.Id = id;
        }

        public override void ValidateForAdd()
        {
            if (this.Name == null)
            {
                throw new Exceptions.BadRequestException(Exceptions.BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "multilingualName");
            }

            this.Name.Validate("multilingualName");

            if (DynamicData?.Count > 0)
            {
                var isEmptyOrNullKeyExist = DynamicData.Any(x => string.IsNullOrEmpty(x.Key));
                if (isEmptyOrNullKeyExist)
                {
                    throw new BadRequestException(BadRequestException.KEY_CANNOT_BE_EMPTY_OR_NULL, "dynamicData");
                }
            }

            if (StartDateInSeconds.HasValue && EndDateInSeconds.HasValue && StartDateInSeconds >= EndDateInSeconds)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDateInSeconds", "endDateInSeconds");
            }

            if(this.UnifiedChannels?.Count > 0)
            {
                foreach (var unifiedChannels in this.UnifiedChannels)
                {
                    unifiedChannels.Validate();
                }
            }
        }

        internal override void ValidateForUpdate()
        {
            if (this.Name != null)
            {
                this.Name.Validate("multilingualName");
            }

            if (DynamicData?.Count > 0)
            {
                var isEmptyOrNullKeyExist = DynamicData.Any(x => string.IsNullOrEmpty(x.Key));
                if (isEmptyOrNullKeyExist)
                {
                    throw new BadRequestException(BadRequestException.KEY_CANNOT_BE_EMPTY_OR_NULL, "dynamicData");
                }
            }

            if (StartDateInSeconds.HasValue && EndDateInSeconds.HasValue && StartDateInSeconds >= EndDateInSeconds)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDateInSeconds", "endDateInSeconds");
            }

            // fill empty feilds
            if (NullableProperties != null && NullableProperties.Contains("unifiedchannels"))
            {
                UnifiedChannels = new List<KalturaUnifiedChannel>();
            }

        }

        public KalturaCategoryItem() : base() { }

        internal override GenericResponse<CategoryItem> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<CategoryItem>(this);
            return CategoryItemHandler.Instance.Add(contextData, coreObject);
        }

        internal override GenericResponse<CategoryItem> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<CategoryItem>(this);
            return CategoryItemHandler.Instance.Update(contextData, coreObject);
        }
    }

    public partial class KalturaCategoryItemListResponse : KalturaListResponse<KalturaCategoryItem>
    {
        public KalturaCategoryItemListResponse() : base() { }
    }
}