using ApiLogic.Base;
using ApiLogic.Catalog;
using Core.Catalog.Handlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public string Name { get; set; }

        /// <summary>
        /// Category parent identifier 
        /// </summary>
        [DataMember(Name = "parentCategoryId")]
        [JsonProperty(PropertyName = "parentCategoryId")]
        [XmlElement(ElementName = "parentCategoryId", IsNullable = true)]
        [SchemeProperty(MinInteger = 1)]
        public long? ParentCategoryId { get; set; }

        /// <summary>
        /// Comma separated list of child categories' Ids.
        /// </summary>
        [DataMember(Name = "childCategoriesIds")]
        [JsonProperty(PropertyName = "childCategoriesIds")]
        [XmlElement(ElementName = "childCategoriesIds")]
        [SchemeProperty(ReadOnly = true)]
        public string ChildCategoriesIds { get; set; }

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

        public List<long> GetChildCategoriesIds()
        {
            if (ChildCategoriesIds != null)
            {
                return GetItemsIn<List<long>, long>(ChildCategoriesIds, "KalturaCategoryItem.childCategoriesIds", true, true);
            }

            return null;
        }        

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
            if (string.IsNullOrEmpty(this.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }
        }

        internal override void ValidateForUpdate()
        {
            if (this.Name !=null && this.Name.Trim() == "")
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }           
        }

        public KalturaCategoryItem() : base() { }

    }

    public partial class KalturaCategoryItemListResponse : KalturaListResponse<KalturaCategoryItem>
    {
        public KalturaCategoryItemListResponse() : base() { }
    }
}