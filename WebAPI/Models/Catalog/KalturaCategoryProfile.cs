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
    public partial class KalturaCategoryProfile : KalturaCrudObject<CategoryProfile, long, CategoryProfileFilter>
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
        ///Comma separated list of images' Ids.
        /// </summary>
        [DataMember(Name = "imagesIds")]
        [JsonProperty(PropertyName = "imagesIds")]
        [XmlArray(ElementName = "imagesIds", IsNullable = true)]
        public string ImagesIds { get; set; }

        public List<long> GetChildCategoriesIds()
        {
            if (ChildCategoriesIds != null)
            {
                return GetItemsIn<List<long>, long>(ChildCategoriesIds, "KalturaCategoryProfile.childCategoriesIds", true, true);
            }

            return null;
        }

        public List<long> GetChannelsIds()
        {
            if (ChannelsIds != null)
            {
                return GetItemsIn<List<long>, long>(ChannelsIds, "KalturaCategoryProfile.channelsIds", true, true);
            }

            return null;
        }

        public List<long> GetImagesIds()
        {
            if (ImagesIds != null)
            {
                return GetItemsIn<List<long>, long>(ImagesIds, "KalturaCategoryProfile.imagesIds", true, true);
            }

            return null;
        }

        internal override ICrudHandler<CategoryProfile, long, CategoryProfileFilter> Handler
        {
            get
            {
                return CategoryProfileHandler.Instance;
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

        public KalturaCategoryProfile() : base() { }

    }

    public partial class KalturaCategoryProfileListResponse : KalturaListResponse<KalturaCategoryProfile>
    {
        public KalturaCategoryProfileListResponse() : base() { }
    }
}