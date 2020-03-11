using ApiLogic.Base;
using ApiLogic.Catalog;
using Core.Catalog.Handlers;
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
    public partial class KalturaCategoryItem : KalturaCrudObject<CategoryItem, long, CategoryItemFilter>
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
        public List<KalturaUnifiedChannel> UnifiedChannels { get; set; }

        /// <summary>
        /// Dynamic data
        /// </summary>
        [DataMember(Name = "dynamicData")]
        [JsonProperty("dynamicData")]
        [XmlElement(ElementName = "dynamicData", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> DynamicData { get; set; }

        /// <summary>
        /// Specifies when was the Category last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        internal override ICrudHandler<CategoryItem, long, CategoryItemFilter> Handler
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

        internal override void ValidateForAdd()
        {
            this.Name.Validate("multilingualName");

            if (DynamicData?.Count > 0)
            {
                var isEmptyOrNullKeyExist = DynamicData.Any(x => string.IsNullOrEmpty(x.Key));
                if (isEmptyOrNullKeyExist)
                {
                    throw new BadRequestException(BadRequestException.KEY_CANNOT_BE_EMPTY_OR_NULL, "dynamicData");
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
        }

        public KalturaCategoryItem() : base() { }

    }

    public partial class KalturaCategoryItemListResponse : KalturaListResponse<KalturaCategoryItem>
    {
        public KalturaCategoryItemListResponse() : base() { }
    }
}