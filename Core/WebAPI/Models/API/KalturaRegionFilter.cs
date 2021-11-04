using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public abstract partial class KalturaBaseRegionFilter : KalturaFilter<KalturaRegionOrderBy>
    {
        internal abstract KalturaRegionListResponse GetRegions(int groupId, KalturaFilterPager pager, KalturaBaseResponseProfile responseProfile = null);
        internal abstract void Validate();

        public override KalturaRegionOrderBy GetDefaultOrderByValue()
        {
            return KalturaRegionOrderBy.CREATE_DATE_ASC;
        }
    }

    public partial class KalturaRegionFilter : KalturaBaseRegionFilter
    {
        /// <summary>
        /// List of comma separated regions external IDs
        /// </summary>
        [DataMember(Name = "externalIdIn")]
        [JsonProperty("externalIdIn")]
        [XmlElement(ElementName = "externalIdIn")]
        public string ExternalIdIn { get; set; }

        /// <summary>
        /// List of comma separated regions Ids
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn")]
        public string IdIn { get; set; }

        /// <summary>
        /// Region parent ID to filter by
        /// </summary>
        [DataMember(Name = "parentIdEqual")]
        [JsonProperty("parentIdEqual")]
        [XmlElement(ElementName = "parentIdEqual")]
        public int ParentIdEqual { get; set; }

        /// <summary>
        /// Region parent ID to filter by
        /// </summary>
        [DataMember(Name = "liveAssetIdEqual")]
        [JsonProperty("liveAssetIdEqual")]
        [XmlElement(ElementName = "liveAssetIdEqual")]
        public int LiveAssetIdEqual { get; set; }

        /// <summary>
        /// Parent region to filter by
        /// </summary>
        [DataMember(Name = "parentOnly")]
        [JsonProperty("parentOnly")]
        [XmlElement(ElementName = "parentOnly")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool ParentOnly { get; set; }

        /// <summary>
        /// Retrieves only the channels belonging specifically to the child region
        /// </summary>
        [DataMember(Name = "exclusiveLcn")]
        [JsonProperty("exclusiveLcn")]
        [XmlElement(ElementName = "exclusiveLcn")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool ExclusiveLcn { get; set; }

        internal override void Validate()
        {

            if ((!string.IsNullOrEmpty(ExternalIdIn) && (!string.IsNullOrEmpty(IdIn) || ParentIdEqual > 0 || ParentOnly == true)) ||
                (!string.IsNullOrEmpty(IdIn) && (!string.IsNullOrEmpty(ExternalIdIn) || ParentIdEqual > 0 || ParentOnly == true)) ||
                (ParentIdEqual > 0 && (!string.IsNullOrEmpty(IdIn) || !string.IsNullOrEmpty(ExternalIdIn) || ParentOnly == true)))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaRegionFilter.externalIdIn, KalturaRegionFilter.idIn", "KalturaRegionFilter.parentIdEqual", "KalturaRegionFilter.parentOnly");
            }
        }

        public List<string> GetExternalIdIn()
        {
            List<string> list = null;
            if (!string.IsNullOrEmpty(ExternalIdIn))
            {
                string[] stringValues = ExternalIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (stringValues != null && stringValues.Length > 0)
                {
                    list = stringValues.ToList();
                }
            }

            return list;
        }

        internal override KalturaRegionListResponse GetRegions(int groupId, KalturaFilterPager pager, KalturaBaseResponseProfile responseProfile = null)
        {
            RegionFilter wsFilter = AutoMapper.Mapper.Map<RegionFilter>(this);

            Func<GenericListResponse<Region>> getListFunc = () =>
                             RegionManager.Instance.GetRegions(groupId, wsFilter, pager.getPageIndex(), pager.getPageSize());

            KalturaGenericListResponse<KalturaRegion> response =
                ClientUtils.GetResponseListFromWS<KalturaRegion, Region>(getListFunc);

            KalturaRegionListResponse result = new KalturaRegionListResponse
            {
                Regions = response.Objects,
                TotalCount = response.TotalCount
            };

            if (response.TotalCount == 0 && responseProfile != null && responseProfile is KalturaDetachedResponseProfile detachedResponseProfile)
            {
                var profile = detachedResponseProfile.RelatedProfiles?.FirstOrDefault(x => x.Filter is KalturaAddDefaultIfEmptyResponseProfile);

                if (profile != null)
                {
                    KalturaDefaultRegionFilter filter = new KalturaDefaultRegionFilter();
                    return filter.GetRegions(groupId, pager);
                }
            }

            return result;
        }
    }

    public partial class KalturaDefaultRegionFilter : KalturaBaseRegionFilter
    {
        internal override KalturaRegionListResponse GetRegions(int groupId, KalturaFilterPager pager, KalturaBaseResponseProfile responseProfile = null)
        {
            var response = ClientsManager.ApiClient().GetDefaultRegion(groupId);
            if (response?.Regions?.Count > 0)
            {
                response.Regions = response.Regions.Where(x => x.IsDefault).ToList();
                response.TotalCount = response.Regions.Count;
            }

            return response;
        }

        internal override void Validate()
        {
        }
    }


    /// <summary>
    /// Kaltura add default if empty ResponseProfile Filter
    /// </summary>
    [SchemeBase(typeof(KalturaRelatedObjectFilter))]
    public partial class KalturaAddDefaultIfEmptyResponseProfile : KalturaFilter<KalturaAddDefaultIfEmptyResponseProfileOrderBy>, KalturaRelatedObjectFilter
    {
        public override KalturaAddDefaultIfEmptyResponseProfileOrderBy GetDefaultOrderByValue()
        {
            return KalturaAddDefaultIfEmptyResponseProfileOrderBy.NONE;
        }
    }

    public enum KalturaAddDefaultIfEmptyResponseProfileOrderBy
    {
        NONE
    }
}