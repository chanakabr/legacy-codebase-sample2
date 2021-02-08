using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public partial class KalturaCategoryVersionFilter : KalturaCrudFilter<KalturaCategoryVersionOrderBy, CategoryVersion>
    {
        public override GenericListResponse<CategoryVersion> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<CategoryVersionFilter>(this);
            return CategoryCache.Instance.ListCategoryVersionDefaults(contextData, coreFilter, pager);
        }

        public override KalturaCategoryVersionOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryVersionOrderBy.UPDATE_DATE_DESC;
        }

        public KalturaCategoryVersionFilter() : base()
        {
        }

        public override void Validate(ContextData contextData)
        {
        }
    }

    [Serializable]
    public partial class KalturaCategoryVersionFilterByTree : KalturaCategoryVersionFilter
    {
        /// <summary>
        /// Category version tree identifier
        /// </summary>
        [DataMember(Name = "treeIdEqual")]
        [JsonProperty("treeIdEqual")]
        [XmlElement(ElementName = "treeIdEqual")]
        [SchemeProperty(MinLong = 1)]
        public long TreeIdEqual { get; set; }

        /// <summary>
        /// Category version state
        /// </summary>
        [DataMember(Name = "stateEqual")]
        [JsonProperty("stateEqual")]
        [XmlElement(ElementName = "stateEqual", IsNullable = true)]
        public KalturaCategoryVersionState? StateEqual { get; set; }

        public override GenericListResponse<CategoryVersion> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<CategoryVersionFilterByTree>(this);
            return CategoryCache.Instance.ListCategoryVersionByTree(contextData, coreFilter, pager);
        }
    }

    public enum KalturaCategoryVersionOrderBy
    {
        UPDATE_DATE_DESC,
        NONE
    }
}