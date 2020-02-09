using ApiLogic.Base;
using ApiLogic.Catalog;
using Core.Catalog.Handlers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
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
        [XmlElement(ElementName = "parentCategoryId")]
        public long? ParentCategoryId { get; set; }

        /// <summary>
        /// Comma separated list of child categories' Ids.
        /// </summary>
        [DataMember(Name = "childCategoriesIds")]
        [JsonProperty(PropertyName = "childCategoriesIds")]
        [XmlArray(ElementName = "childCategoriesIds", IsNullable = true)]
        public string ChildCategoriesIds { get; set; }

        /// <summary>
        /// Comma separated list of channels' Ids.
        /// </summary>
        [DataMember(Name = "channelsIds")]
        [JsonProperty(PropertyName = "channelsIds")]
        [XmlArray(ElementName = "channelsIds", IsNullable = true)]
        public string ChannelsIds { get; set; }

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

        public List<long> GetChannelsIds()
        {
            if (ChannelsIds != null)
            {
                return GetItemsIn<List<long>, long>(ChannelsIds, "KalturaCategoryItem.channelsIds", true, true);
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
            throw new System.NotImplementedException();
        }

        internal override void ValidateForAdd()
        {
            throw new System.NotImplementedException();
        }

        internal override void ValidateForUpdate()
        {
            throw new System.NotImplementedException();
        }

        public KalturaCategoryItem() : base() { }

    }

    public partial class KalturaCategoryItemListResponse : KalturaListResponse<KalturaCategoryItem>
    {
        public KalturaCategoryItemListResponse() : base() { }
    }
}