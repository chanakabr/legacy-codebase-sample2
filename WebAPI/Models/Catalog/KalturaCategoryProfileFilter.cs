using ApiLogic.Base;
using ApiLogic.Catalog;
using Core.Catalog.Handlers;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    [Serializable]
    public partial class KalturaCategoryProfileFilter : KalturaCrudFilter<KalturaCategoryProfileOrderBy, CategoryProfile, long, CategoryProfileFilter>
    {
        /// <summary>
        /// Indicates which category profile list to return by their category profile identifier.
        /// </summary>
        [DataMember(Name = "categoryProfileIdEqual")]
        [JsonProperty("categoryProfileIdEqual")]
        [XmlElement(ElementName = "categoryProfileIdEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(MinInteger = 1)]
        public long CategoryProfileIdEqual { get; set; }

        /// <summary>
        /// Indicates which category profile list to return by their category profile name.
        /// </summary>
        [DataMember(Name = "categoryProfileNameEqual")]
        [JsonProperty("categoryProfileNameEqual")]
        [XmlElement(ElementName = "categoryProfileNameEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string CategoryProfileNameEqual { get; set; }

        public override ICrudHandler<CategoryProfile, long, CategoryProfileFilter> Handler
        {
            get
            {
                return CategoryProfileHandler.Instance;
            }
        }
        public override KalturaCategoryProfileOrderBy GetDefaultOrderByValue()
        {
            return KalturaCategoryProfileOrderBy.NONE;
        }

        public override void Validate()
        {
        }

        public KalturaCategoryProfileFilter() : base()
        {
        }
    }

    public enum KalturaCategoryProfileOrderBy
    {
        NONE
    }
}
