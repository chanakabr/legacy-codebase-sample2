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

        public override GenericListResponse<DynamicList> List(ContextData contextData, CorePager pager)
        {
            throw new NotImplementedException();
        }

        public override void Validate(ContextData contextData)
        {
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

        public override void Validate(ContextData contextData)
        {
            if (string.IsNullOrEmpty(IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }

            var items = GetItemsIn<List<long>, long>(this.IdIn, "idIn", true);
            if (items.Count > 500)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "KalturaDynamicListIdInFilter.idIn", 500);
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
    public abstract partial class KalturaDynamicListSearchFilter : KalturaDynamicListFilter
    {
        /// <summary>
        /// DynamicList id to search by
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLong = 1)]
        public long? IdEqual { get; set; }

        /// <summary>
        /// udid value that should be in the DynamicList
        /// </summary>
        [DataMember(Name = "valueEqual")]
        [JsonProperty("valueEqual")]
        [XmlElement(ElementName = "valueEqual", IsNullable = true)]
        [SchemeProperty(IsNullable = true, MinLength = 1)]
        public string ValueEqual { get; set; }

        public override void Validate(ContextData contextData)
        {
            if (this.IdEqual.HasValue && string.IsNullOrEmpty(this.ValueEqual))
            {
                throw new BadRequestException(BadRequestException.BOTH_ARGUMENTS_MUST_HAVE_VALUE, "valueEqual", "idEqual");
            }

            if (!string.IsNullOrEmpty(this.ValueEqual) && !this.IdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.BOTH_ARGUMENTS_MUST_HAVE_VALUE, "idEqual", "valueEqual");
            }
        }
    }

    /// <summary>
    /// UdidDynamicListSearchFilter
    /// </summary>
    public partial class KalturaUdidDynamicListSearchFilter : KalturaDynamicListSearchFilter
    {
        public override GenericListResponse<DynamicList> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<DynamicListSearchFilter>(this);
            coreFilter.TypeEqual = DynamicListType.UDID;
            return DynamicListManager.Instance.SearchDynamicLists(contextData, coreFilter, pager);
        }
    }
}