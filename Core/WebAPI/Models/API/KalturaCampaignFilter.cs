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
        }

        public KalturaCampaignIdInFilter() : base()
        {
        }

        public override GenericListResponse<Campaign> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<CampaignIdInFilter>(this);
            return CampaignManager.Instance.List(contextData, coreFilter, pager);
        }
    }
}