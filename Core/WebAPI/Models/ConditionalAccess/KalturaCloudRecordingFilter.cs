using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using System.Collections.Generic;
using WebAPI.ClientManagers.Client;
using ApiObjects.Base;
using ApiObjects;
using System.Linq;
using TVinciShared;
using Core.Catalog.Response;
using ApiObjects.Response;
using Core.Catalog.Request;
using WebAPI.ClientManagers;
using Core.Catalog;
using WebAPI.Models.Catalog;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Filtering cloud external recordings
    /// </summary>
    [Serializable]
    public partial class KalturaCloudRecordingFilter : KalturaExternalRecordingFilter
    {
        private static readonly Type relatedObjectFilterType = typeof(KalturaExternalRecordingResponseProfileFilter);
        
        /// <summary>
        /// Adapter Data
        /// </summary>
        [DataMember(Name = "adapterData")]
        [JsonProperty(PropertyName = "adapterData")]
        [XmlArray(ElementName = "adapterData", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public SerializableDictionary<string, KalturaStringValue> AdapterData { get; set; }

        internal override KalturaRecordingListResponse SearchRecordings(ContextData contextData, KalturaFilterPager pager)
        {
            Dictionary<string, string> adapterData = null;
            if (this.AdapterData != null)
            {
                adapterData = this.AdapterData.ToDictionary(x => x.Key.ToLower(), x => x.Value.value.ToLowerOrNull());
            }

            var response = ClientsManager.ConditionalAccessClient().SearchCloudRecordings(contextData.GroupId, contextData.UserId.Value.ToString(), contextData.DomainId.Value,
                adapterData, this.ConvertStatusIn(), pager.getPageIndex(), pager.PageSize);

            var responseProfile = Utils.Utils.GetResponseProfileFromRequest();
            if (response.Objects != null && response.Objects.Count > 0 && responseProfile != null && responseProfile is KalturaDetachedResponseProfile detachedResponseProfile)
            {
                var profile = detachedResponseProfile.RelatedProfiles.FirstOrDefault(x => x.Filter.GetType() == relatedObjectFilterType);

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

        private Dictionary<long?, KalturaAssetListResponse> GetRelatedAssetsMap(ContextData contextData, List<KalturaRecording> recordings)
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