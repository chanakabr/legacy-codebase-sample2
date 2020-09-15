using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    public enum KalturaDynamicListOrderBy
    {
        NONE
    }

    /// <summary>
    /// DynamicListFilter
    /// </summary>
    public partial class KalturaDynamicListFilter : KalturaCrudFilter<KalturaDynamicListOrderBy, DynamicList>
    {
        public KalturaDynamicListFilter() : base() { }

        public override KalturaDynamicListOrderBy GetDefaultOrderByValue()
        {
            return KalturaDynamicListOrderBy.NONE;
        }

        public override void Validate() { }

        public override GenericListResponse<DynamicList> List(ContextData contextData, CorePager pager)
        {
            var filter = new KalturaDynamicListSearchFilter();
            return filter.List(contextData, pager);
        }
    }

    /// <summary>
    /// DynamicListIdInFilter
    /// </summary>
    public partial class KalturaDynamicListIdInFilter : KalturaDynamicListFilter
    {
        /// <summary>
        /// DynamicList identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string IdIn { get; set; }

        public override void Validate()
        {
            if (string.IsNullOrEmpty(IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }

            var items = GetItemsIn<List<long>, long>(this.IdIn, "idIn", true);
            if (items.Count > 500)
            {
                throw new BadRequestException(BadRequestException.MAX_ARGUMENTS, "KalturaDynamicListIdInFilter.idIn", 500);
            }
        }

        public override GenericListResponse<DynamicList> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<DynamicListnIdInFilter>(this);
            return DynamicListManager.Instance.GetDynamicListsByIds(contextData, coreFilter);
        }
    }

    /// <summary>
    /// DynamicListSearchFilter
    /// </summary>
    public partial class KalturaDynamicListSearchFilter : KalturaDynamicListFilter
    {
        /// <summary>
        /// Comma-separated String which represent List of objects that is in the dynamicList.
        /// </summary>
        [DataMember(Name = "valueIn")]
        [JsonProperty("valueIn")]
        [XmlElement(ElementName = "valueIn", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public string ValueIn { get; set; }

        public override GenericListResponse<DynamicList> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<DynamicListSearchFilter>(this);
            return DynamicListManager.Instance.SearchDynamicLists(contextData, coreFilter, pager);
        }
    }

    /// <summary>
    /// UdidDynamicListSearchFilter
    /// </summary>
    public partial class KalturaUdidDynamicListSearchFilter : KalturaDynamicListFilter
    {
        public override GenericListResponse<DynamicList> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<DynamicListSearchFilter>(this);
            return DynamicListManager.Instance.SearchDynamicLists(contextData, coreFilter, pager);
        }
    }
}
