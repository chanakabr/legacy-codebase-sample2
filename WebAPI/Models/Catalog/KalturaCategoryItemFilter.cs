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
        /// <summary>
        /// Category item identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        public string IdIn { get; set; }
        

        /// <summary>
        /// KSQL expression
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Ksql { get; set; }

        /// <summary>
        /// Root only to filter by
        /// </summary>
        [DataMember(Name = "rootOnly")]
        [JsonProperty("rootOnly")]
        [XmlElement(ElementName = "rootOnly")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool RootOnly { get; set; }

        public override ICrudHandler<CategoryItem, long, CategoryItemFilter> Handler
        {
            get
            {
                return CategoryItemHandler.Instance;
            }
        }
        public override KalturaCategoryItemOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryItemOrderBy.NONE;
        }

        public override void Validate()
        {
        }

        public KalturaCategoryItemFilter() : base()
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
    }

    public enum KalturaCategoryItemOrderBy
    {
        NONE
    }
}
