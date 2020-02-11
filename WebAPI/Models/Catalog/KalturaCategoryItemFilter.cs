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
    [Serializable]
    public partial class KalturaCategoryItemFilter : KalturaCrudFilter<KalturaCategoryItemOrderBy, CategoryItem, long, CategoryItemFilter>
    {
        internal virtual KalturaCategoryItemListResponse GetCategoryItems(int groupId, KalturaFilterPager pager)
        {
            return null;
        }
        
        public override KalturaCategoryItemOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryItemOrderBy.NONE;
        }

        public override void Validate()
        {
        }

        public override ICrudHandler<CategoryItem, long, CategoryItemFilter> Handler
        {
            get
            {
                return CategoryItemHandler.Instance;
            }
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
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        public string IdIn { get; set; }              
       
        public override KalturaCategoryItemOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryItemOrderBy.NONE;
        }

        public override void Validate()
        {
        }

        public KalturaCategoryItemByIdInFilter() : base()
        {
        }

        public List<long> GetIdIn()
        {
            if (IdIn != null)
            {
                return GetItemsIn<List<long>, long>(IdIn, "KalturaCategoryItemFilter.idIn", true, true);
            }

            return null;
        }

        internal override KalturaCategoryItemListResponse GetCategoryItems(int groupId, KalturaFilterPager pager)
        {
            throw new NotImplementedException();
        }
    }

    public partial class KalturaCategoryItemByKsqlFilter : KalturaCategoryItemFilter
    {
        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        public override KalturaCategoryItemOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryItemOrderBy.NONE;
        }

        public override void Validate()
        {
        }

        public KalturaCategoryItemByKsqlFilter() : base()
        {
        }       

        internal override KalturaCategoryItemListResponse GetCategoryItems(int groupId, KalturaFilterPager pager)
        {
            throw new NotImplementedException();
        }
    }

    public partial class KalturaCategoryItemByRootFilter : KalturaCategoryItemFilter
    {
        public override KalturaCategoryItemOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryItemOrderBy.NONE;
        }

        public override void Validate()
        {
        }

        public KalturaCategoryItemByRootFilter() : base()
        {
        }

        internal override KalturaCategoryItemListResponse GetCategoryItems(int groupId, KalturaFilterPager pager)
        {
            throw new NotImplementedException();
        }
    }

    public enum KalturaCategoryItemOrderBy
    {
        NONE
    }
}
