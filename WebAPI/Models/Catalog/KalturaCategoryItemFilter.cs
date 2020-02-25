using ApiLogic.Base;
using ApiLogic.Catalog;
using ApiObjects.Base;
using ApiObjects.Response;
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
    [Serializable]
    public partial class KalturaCategoryItemFilter : KalturaCrudFilter<KalturaCategoryItemOrderBy, CategoryItem>
    {
        public override GenericListResponse<CategoryItem> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<CategoryItemFilter>(this);
            return CategoryItemHandler.Instance.List(contextData, coreFilter, pager);
        }

        public override KalturaCategoryItemOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryItemOrderBy.CREATE_DATE_ASC;
        }

        public override void Validate()
        {
        }

        public KalturaCategoryItemFilter() : base()
        {

        }
    }

    public partial class KalturaCategoryItemByIdInFilter : KalturaCategoryItemFilter
    {
        /// <summary>
        /// Category item identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = false)]
        public string IdIn { get; set; }              
       
        public override KalturaCategoryItemOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryItemOrderBy.CREATE_DATE_ASC;
        }

        public override void Validate()
        {
            if(string.IsNullOrEmpty(IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }
        }

        public KalturaCategoryItemByIdInFilter() : base()
        {
        }

        public override GenericListResponse<CategoryItem> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<CategoryItemByIdInFilter>(this);
            return CategoryItemHandler.Instance.List(contextData, coreFilter, pager);
        }
    }

    public partial class KalturaCategoryItemSearchFilter : KalturaCategoryItemFilter
    {
        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        /// <summary>
        /// Root only
        /// </summary>
        [DataMember(Name = "rootOnly")]
        [JsonProperty("rootOnly")]
        [XmlElement(ElementName = "rootOnly")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool RootOnly { get; set; }

        public override KalturaCategoryItemOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryItemOrderBy.CREATE_DATE_ASC;
        }

        public override void Validate()
        {           
        }

        public KalturaCategoryItemSearchFilter() : base()
        {
        }

        public override GenericListResponse<CategoryItem> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<CategoryItemSearchFilter>(this);
            return CategoryItemHandler.Instance.List(contextData, coreFilter, pager);
        }
    }

    public enum KalturaCategoryItemOrderBy
    {
        NAME_ASC,
        NAME_DESC,
        CREATE_DATE_ASC,
        CREATE_DATE_DESC
    }

    public partial class KalturaCategoryItemAncestorsFilter : KalturaCategoryItemFilter
    {
        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public long Id { get; set; }

        public override KalturaCategoryItemOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryItemOrderBy.CREATE_DATE_ASC;
        }

        public override void Validate()
        {
        }

        public KalturaCategoryItemAncestorsFilter() : base()
        {
        }

        public override GenericListResponse<CategoryItem> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<CategoryItemAncestorsFilter>(this);
            return CategoryItemHandler.Instance.List(contextData, coreFilter, pager);
        }
    }


}