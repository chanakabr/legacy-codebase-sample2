using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Base;
using System;
using ApiLogic.Users.Managers;

namespace WebAPI.Models.API
{
    public enum KalturaCampaignOrderBy
    {
        NONE
    }

    /// <summary>
    /// Business module rule filter
    /// </summary>
    [Serializable]
    public partial class KalturaCampaignFilter : KalturaCrudFilter<KalturaCampaignOrderBy, Campaign>
    {
        /// <summary>
        /// discount module id the campain applied on
        /// </summary>
        [DataMember(Name = "discountModuleIdApplied")]
        [JsonProperty("discountModuleIdApplied")]
        [XmlElement(ElementName = "discountModuleIdApplied", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public long? DiscountModuleIdApplied { get; set; }

        // TODO SHIR - CHECK PROP TO FILTER BY

        public KalturaCampaignFilter() : base()
        {
        }

        public override KalturaCampaignOrderBy GetDefaultOrderByValue()
        {
            return KalturaCampaignOrderBy.NONE;
        }

        public override GenericListResponse<Campaign> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<CampaignFilter>(this);
            return CampaignManager.Instance.List(contextData, coreFilter, pager);
        }

        public override void Validate()
        {
            // TODO SHIR
        }
    }
}