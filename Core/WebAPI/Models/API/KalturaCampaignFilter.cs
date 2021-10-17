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
        START_DATE_DESC
    }

    /// <summary>
    /// Campaign filter (same as KalturaCampaignSearchFilter with no parameters)
    /// </summary>
    [Serializable]
    public partial class KalturaCampaignFilter : KalturaCrudFilter<KalturaCampaignOrderBy, Campaign>
    {
        public KalturaCampaignFilter() : base() { }

        public override KalturaCampaignOrderBy GetDefaultOrderByValue()
        {
            return KalturaCampaignOrderBy.START_DATE_DESC;
        }

        public override void Validate(ContextData contextData) 
        {
            bool isAllowedToViewInactiveCampaigns = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, contextData.UserId.ToString(), true);
            if (!isAllowedToViewInactiveCampaigns)
            {
                throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);
            }
        }

        public override GenericListResponse<Campaign> List(ContextData contextData, CorePager pager)
        {
            var filter = new KalturaCampaignSearchFilter();
            return filter.List(contextData, pager);
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
        
        public override void Validate(ContextData contextData)
        {
            if (string.IsNullOrEmpty(IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }

            var items = GetItemsIn<List<long>, long>(this.IdIn, "idIn", true);
            if (items.Count > 500)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "KalturaCampaignIdInFilter.idIn", 500);
            }
        }

        public override GenericListResponse<Campaign> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<CampaignIdInFilter>(this);
            coreFilter.IsAllowedToViewInactiveCampaigns = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, contextData.UserId.ToString(), true);
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
        /// has Promotion
        /// </summary>
        [DataMember(Name = "hasPromotion")]
        [JsonProperty("hasPromotion")]
        [XmlElement(ElementName = "hasPromotion", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool? HasPromotion { get; set; }

        public override GenericListResponse<Campaign> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<CampaignSearchFilter>(this);
            return CampaignManager.Instance.SearchCampaigns(contextData, coreFilter, pager);
        }
    }

    public partial class KalturaTriggerCampaignSearchFilter : KalturaCampaignSearchFilter
    {
        public override GenericListResponse<Campaign> List(ContextData contextData, CorePager pager)
        {
            var response = new GenericListResponse<Campaign>();
            var coreFilter = AutoMapper.Mapper.Map<TriggerCampaignFilter>(this);
            var triggerCampaignResponse = CampaignManager.Instance.ListTriggerCampaigns(contextData, coreFilter, pager);

            response.SetStatus(triggerCampaignResponse.Status);

            if (!response.IsOkStatusCode())
            {    
                return response;
            }

            response.Objects.AddRange(triggerCampaignResponse.Objects);
            response.TotalItems = triggerCampaignResponse.TotalItems;
            return response;
        }
    }

    public partial class KalturaBatchCampaignSearchFilter : KalturaCampaignSearchFilter
    {
        public override GenericListResponse<Campaign> List(ContextData contextData, CorePager pager)
        {
            var response = new GenericListResponse<Campaign>();
            var coreFilter = AutoMapper.Mapper.Map<BatchCampaignFilter>(this);
            var batchCampaignResponse = CampaignManager.Instance.ListBatchCampaigns(contextData, coreFilter, pager);

            response.SetStatus(batchCampaignResponse.Status);

            if (!response.IsOkStatusCode())
            {
                return response;
            }

            response.Objects.AddRange(batchCampaignResponse.Objects);
            response.TotalItems = batchCampaignResponse.TotalItems;
            return response;
        }
    }
}