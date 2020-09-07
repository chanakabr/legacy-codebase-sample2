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
using System.Collections.Generic;

namespace WebAPI.Models.API
{
    public enum KalturaCampaignOrderBy
    {
        NONE
    }

    /// <summary>
    /// Campaign filter (same as KalturaCampaignSearchFilter with no parameters)
    /// </summary>
    [Serializable]
    public partial class KalturaCampaignFilter : KalturaCrudFilter<KalturaCampaignOrderBy, Campaign>
    {
        public KalturaCampaignFilter() : base()
        {
        }

        public override KalturaCampaignOrderBy GetDefaultOrderByValue()
        {
            return KalturaCampaignOrderBy.NONE;
        }

        public override void Validate()
        {
        }

        public override GenericListResponse<Campaign> List(ContextData contextData, CorePager pager)
        {
            // TODO SHIR - LIST ALL
            var coreFilter = AutoMapper.Mapper.Map<CampaignFilter>(this);
            //return CampaignManager.Instance.List(contextData, coreFilter, pager);
            return null;
        }
    }

    public partial class KalturaCampaignIdInFilter : KalturaCampaignFilter
    {
        /// <summary>
        /// campaign identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = false)]
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
                throw new BadRequestException(BadRequestException.MAX_ARGUMENTS, "KalturaCampaignIdInFilter.idIn", 500);
            }
        }

        public override GenericListResponse<Campaign> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<CampaignIdInFilter>(this);
            return CampaignManager.Instance.ListCampaingsByIds(contextData, coreFilter);
        }
    }

    public partial class KalturaCampaignSearchFilter : KalturaCampaignFilter
    {
        /// <summary>
        /// start Date Greater Than Or Equal
        /// </summary>
        [DataMember(Name = "startDateGreaterThanOrEqual")]
        [JsonProperty("startDateGreaterThanOrEqual")]
        [XmlElement(ElementName = "startDateGreaterThanOrEqual", IsNullable = true)]
        public long? StartDateGreaterThanOrEqual { get; set; }

        /// <summary>
        /// end Date Greater Than Or Equal
        /// </summary>
        [DataMember(Name = "endDateLessThanOrEqual")]
        [JsonProperty("endDateLessThanOrEqual")]
        [XmlElement(ElementName = "endDateLessThanOrEqual", IsNullable = true)]
        public long? EndDateLessThanOrEqual { get; set; }

        /// <summary>
        /// state Equal
        /// </summary>
        [DataMember(Name = "stateEqual")]
        [JsonProperty("stateEqual")]
        [XmlElement(ElementName = "stateEqual", IsNullable = true)]
        public KalturaObjectState? StateEqual { get; set; }

        /// <summary>
        /// Contain Discount Model
        /// </summary>
        [DataMember(Name = "containDiscountModel")]
        [JsonProperty("containDiscountModel")]
        [XmlElement(ElementName = "containDiscountModel", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool? ContainDiscountModel { get; set; }

        public override GenericListResponse<Campaign> List(ContextData contextData, CorePager pager)
        {
            // TODO SHIR - LIST ALL
            var coreFilter = AutoMapper.Mapper.Map<CampaignSearchFilter>(this);
            //return CampaignManager.Instance.List(contextData, coreFilter, pager);
            return null;
        }
    }

    public partial class KalturaTriggerCampaignSearchFilter : KalturaCampaignFilter
    {
        public override GenericListResponse<Campaign> List(ContextData contextData, CorePager pager)
        {
            // TODO SHIR - LIST TRIGGER
            var coreFilter = AutoMapper.Mapper.Map<TriggerCampaignFilter>(this);
            //return CampaignManager.Instance.ListTriggerCampaigns(contextData, coreFilter, pager);
            return null;
        }
    }

    public partial class KalturaBatchCampaignSearchFilter : KalturaCampaignFilter
    {
        public override GenericListResponse<Campaign> List(ContextData contextData, CorePager pager)
        {
            // TODO SHIR - LIST BATCH
            var coreFilter = AutoMapper.Mapper.Map<BatchCampaignFilter>(this);
            //return CampaignManager.Instance.ListBatchCampaigns(contextData, coreFilter, pager);
            return null;
        }
    }
}