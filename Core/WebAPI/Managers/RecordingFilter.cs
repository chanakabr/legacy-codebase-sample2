using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using AutoMapper;
using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.Managers
{
    public class RecordingFilter
    {
        private static readonly Type RelatedObjectFilterTypeForCloudRecordingFilter = typeof(KalturaExternalRecordingResponseProfileFilter);

        public static KalturaRecordingListResponse SearchRecordings(KalturaRecordingFilter filter, ContextData contextData, KalturaFilterPager pager)
        {
            switch (filter)
            {
                case KalturaCloudRecordingFilter f: return SearchRecordingsByCloudRecordingFilter(f, contextData, pager);
                case KalturaExternalRecordingFilter f: return SearchRecordingsByExternalRecordingFilter(f, contextData, pager);
                case KalturaRecordingFilter f: return SearchRecordingsByRecordingFilter(f, contextData, pager);
                default: throw new NotImplementedException($"List for {filter.objectType} is not implemented");
            }
        }

        private static KalturaRecordingListResponse SearchRecordingsByRecordingFilter(KalturaRecordingFilter filter, ContextData contextData, KalturaFilterPager pager)
        {
            filter.Validate();

            var response = ClientsManager.ConditionalAccessClient().SearchRecordings(contextData.GroupId, contextData.UserId.Value.ToString(), contextData.DomainId.Value,
                filter.ConvertStatusIn(), filter.Ksql, filter.GetExternalRecordingIds(), pager.GetRealPageIndex(), pager.PageSize, filter.OrderBy, null);

            return response;
        }

        private static KalturaRecordingListResponse SearchRecordingsByExternalRecordingFilter(KalturaExternalRecordingFilter filter, ContextData contextData, KalturaFilterPager pager)
        {
            filter.Validate();

            var metaDataFilter = filter.MetaData.ToDictionary(x => x.Key.ToLower(), x => x.Value.value.ToLowerOrNull());

            var response = ClientsManager.ConditionalAccessClient().SearchRecordings(contextData.GroupId, contextData.UserId.Value.ToString(), contextData.DomainId.Value,
                filter.ConvertStatusIn(), filter.Ksql, filter.GetExternalRecordingIds(), pager.GetRealPageIndex(), pager.PageSize, filter.OrderBy, metaDataFilter);

            return response;
        }

        private static KalturaRecordingListResponse SearchRecordingsByCloudRecordingFilter(KalturaCloudRecordingFilter filter, ContextData contextData, KalturaFilterPager pager)
        {
            Dictionary<string, string> adapterData = null;
            if (filter.AdapterData != null)
            {
                adapterData = filter.AdapterData.ToDictionary(x => x.Key.ToLower(), x => x.Value.value);
            }

            var response = ClientsManager.ConditionalAccessClient().SearchCloudRecordings(contextData.GroupId, contextData.UserId.Value.ToString(), contextData.DomainId.Value,
                adapterData, filter.ConvertStatusIn(), pager.GetRealPageIndex(), pager.PageSize);

            var responseProfile = Utils.Utils.GetResponseProfileFromRequest();
            if (response.Objects != null && response.Objects.Count > 0 && responseProfile != null && responseProfile is KalturaDetachedResponseProfile detachedResponseProfile)
            {
                var profile = detachedResponseProfile.RelatedProfiles.FirstOrDefault(x => x.Filter.GetType() == RelatedObjectFilterTypeForCloudRecordingFilter);

                if (profile != null && !string.IsNullOrEmpty(profile.Name))
                {
                    var relatedAssetsMap = GetRelatedAssetsMap(contextData, response.Objects);

                    foreach (var recording in response.Objects)
                    {
                        if (relatedAssetsMap.ContainsKey(recording.AssetId))
                        {
                            if (recording.relatedObjects == null)
                            {
                                recording.relatedObjects = new SerializableDictionary<string, IKalturaListResponse>();
                            }

                            if (!recording.relatedObjects.ContainsKey(profile.Name))
                            {
                                recording.relatedObjects.Add(profile.Name, relatedAssetsMap[recording.AssetId]);
                            }
                        }
                    }
                }
            }

            return response;
        }

        private static Dictionary<long?, KalturaAssetListResponse> GetRelatedAssetsMap(ContextData contextData, List<KalturaRecording> recordings)
        {
            var searchResponse = new UnifiedSearchResponse();
            searchResponse.status.Code = (int)eResponseStatus.OK;
            searchResponse.searchResults = new List<UnifiedSearchResult>(recordings.Where(x => x.AssetId > 0)
                .Select(x => new UnifiedSearchResult() { AssetId = x.AssetId.ToString(), AssetType = eAssetTypes.EPG }));
            searchResponse.m_nTotalItems = searchResponse.searchResults.Count;

            var group = GroupsManager.Instance.GetGroup(contextData.GroupId);
            var request = new UnifiedSearchRequest()
            {
                m_oFilter = new Filter()
                {
                    m_sDeviceId = contextData.Udid,
                    m_nLanguage = Utils.Utils.GetLanguageId(contextData.GroupId, Utils.Utils.GetLanguageFromRequest()),
                    m_bUseStartDate = group.UseStartDate,
                    m_bOnlyActiveMedia = group.GetOnlyActiveAssets
                },
                m_nGroupID = contextData.GroupId
            };

            var isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, contextData.UserId.Value.ToString(), true);
            var format = Utils.Utils.GetFormatFromRequest();
            var managementData = !string.IsNullOrEmpty(format) && format == "30" ? true : false;

            var assetsResponse = ClientsManager.CatalogClient().GetAssetFromUnifiedSearchResponse
                (contextData.GroupId, searchResponse, request, isAllowedToViewInactiveAssets, managementData);

            var assetsMap = assetsResponse.Objects == null ? new Dictionary<long?, KalturaAssetListResponse>() :
                assetsResponse.Objects.ToDictionary(x => x.Id, y => new KalturaAssetListResponse() { Objects = new List<KalturaAsset>() { y }, TotalCount = 1 });

            return assetsMap;
        }
    }
}
