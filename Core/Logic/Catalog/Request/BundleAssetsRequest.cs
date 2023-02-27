using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog.Cache;
using Core.Catalog.Response;
using GroupsCacheManager;
using Phx.Lib.Log;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Core.Catalog.Request
{
    [DataContract]
    public class BundleAssetsRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public eBundleType m_eBundleType;
        [DataMember]
        public int m_nBundleID;
        [DataMember]
        public OrderObj m_oOrderObj;
        [DataMember]
        public string m_sMediaType;
        [DataMember]
        public bool isAllowedToViewInactiveAssets;
        [DataMember]
        public string AssetFilterKsql;

        private const string SUB_DATA_TABLE = "subscriptions";
        private const string COL_DATA_TABLE = "collections";

        public BundleAssetsRequest()
            : base()
        {
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                BundleAssetsRequest request = (BundleAssetsRequest)oBaseRequest;
                List<SearchResult> lMedias = new List<SearchResult>();
                UnifiedSearchResponse response = new UnifiedSearchResponse();
                response.status = new ApiObjects.Response.Status();

                if (request == null || request.m_nBundleID == 0)
                    throw new Exception("request object is null or Required variables is null");

                CheckSignature(request);

                bool doesGroupUsesTemplates = CatalogManagement.CatalogManager.Instance.DoesGroupUsesTemplates(request.m_nGroupID);
                int parentGroupId = request.m_nGroupID;
                Group groupInCache = null;
                GroupsCacheManager.GroupManager groupManager = null;
                if (!doesGroupUsesTemplates)
                {
                    // Get group from cache
                    groupManager = new GroupsCacheManager.GroupManager();
                    CatalogCache catalogCache = CatalogCache.Instance();
                    parentGroupId = catalogCache.GetParentGroup(request.m_nGroupID);
                    groupInCache = groupManager.GetGroup(parentGroupId);
                }

                // Get channel IDs of current bundle
                List<int> channelIds = CatalogLogic.GetBundleChannelIds(parentGroupId, request.m_nBundleID, request.m_eBundleType);
                List<GroupsCacheManager.Channel> allChannels = new List<GroupsCacheManager.Channel>();

                if (channelIds == null)
                    channelIds = new List<int>();
                
                if (doesGroupUsesTemplates)
                {
                    long userId = 0;
                    long.TryParse(m_sSiteGuid, out userId);

                    GenericListResponse<GroupsCacheManager.Channel> channelRes = CatalogManagement.ChannelManager.Instance.SearchChannels(
                        parentGroupId, true, string.Empty, channelIds, 0, channelIds.Count, ChannelOrderBy.Id,
                        ApiObjects.SearchObjects.OrderDir.ASC, false, userId);
                    if (channelRes.HasObjects())
                    {
                        allChannels.AddRange(channelRes.Objects);
                    }
                }
                else
                {
                    allChannels.AddRange(groupManager.GetChannels(channelIds, parentGroupId));
                }

                if (channelIds != null && channelIds.Count > 0)
                {
                    if (allChannels.Count > 0)
                    {
                        string[] sMediaTypesFromRequest;

                        if (string.IsNullOrEmpty(request.m_sMediaType))
                        {
                            sMediaTypesFromRequest = new string[1] { "0" };
                        }
                        else
                        {
                            if (request.m_sMediaType.EndsWith(";"))
                            {
                                request.m_sMediaType = request.m_sMediaType.Remove(request.m_sMediaType.Length - 1);
                            }

                            sMediaTypesFromRequest = request.m_sMediaType.Split(';');
                        }

                        List<BaseSearchObject> searchObjectsList = BuildBaseSearchObjects(
                            request,
                            groupInCache,
                            allChannels,
                            sMediaTypesFromRequest,
                            parentGroupId,
                            doesGroupUsesTemplates,
                            request.isAllowedToViewInactiveAssets,
                            request.AssetFilterKsql);

                        if (searchObjectsList != null && searchObjectsList.Count > 0)
                        {
                            try
                            {
                                IIndexManager indexManager = IndexManagerFactory.Instance.GetIndexManager(parentGroupId);

                                ApiObjects.SearchObjects.OrderObj oSearchOrder = new ApiObjects.SearchObjects.OrderObj();
                                if (request.m_oOrderObj == null)
                                {
                                    oSearchOrder.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.CREATE_DATE;
                                    oSearchOrder.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                                }
                                CatalogLogic.GetOrderValues(ref oSearchOrder, request.m_oOrderObj);
                                if (oSearchOrder.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.META && string.IsNullOrEmpty(oSearchOrder.m_sOrderValue))
                                {
                                    oSearchOrder.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.CREATE_DATE;
                                    oSearchOrder.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                                }

                                int totalItems = 0;
                                var searchResults =
                                    indexManager.SearchSubscriptionAssets(
                                        searchObjectsList, request.m_oFilter.m_nLanguage, request.m_oFilter.m_bUseStartDate,
                                        request.m_sMediaType, oSearchOrder, request.m_nPageIndex, request.m_nPageSize, ref totalItems);

                                if (searchResults != null)
                                {
                                    response.m_nTotalItems = totalItems;
                                    response.searchResults = searchResults;
                                }
                            }
                            catch (Exception ex)
                            {
                                response.status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Search bundle failed");
                                log.Error(ex.Message);
                            }
                        }
                    }
                }

                log.Debug("Info - BundleMediaRequest - total returned items = " + response.m_nTotalItems);
                return (BaseResponse)response;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                throw ex;
            }
        }

        public static List<BaseSearchObject> BuildBaseSearchObjects(
            BaseRequest request,
            Group groupInCache, 
            List<Channel> allChannels,
            string[] mediaTypes,
            int groupId,
            bool doesGroupUsesTemplates,
            bool isAllowedToViewInactiveAssets,
            string assetFilterKsql)
        {
            List<BaseSearchObject> searchObjectsList = new List<BaseSearchObject>();

            // Validate all channels parameter
            if (allChannels == null || allChannels.Count == 0)
            {
                return searchObjectsList;
            }

            BaseSearchObject[] searchObjectsArray = new BaseSearchObject[allChannels.Count];
            Task[] channelsSearchObjectTasks = new Task[allChannels.Count];

            // save monitor and logs context data
            LogContextData contextData = new LogContextData();

            // Building search object for each channel
            for (int searchObjectIndex = 0; searchObjectIndex < allChannels.Count; searchObjectIndex++)
            {
                channelsSearchObjectTasks[searchObjectIndex] = new Task(
                     (obj) =>
                     {
                         // load monitor and logs context data
                         contextData.Load();

                         try
                         {
                             int channelIndex = (int)obj;

                             if (doesGroupUsesTemplates || groupInCache != null)
                             {
                                 GroupsCacheManager.Channel currentChannel = allChannels[channelIndex];

                                 var typeIntersection = currentChannel.m_nMediaType.Select(t => t.ToString()).Intersect(mediaTypes);

                                 if (
                                     // if we want all media types in request 
                                     mediaTypes.Contains<string>("0") ||
                                     // or if current channel is defined for all media types
                                     currentChannel.m_nMediaType == null || currentChannel.m_nMediaType.Count == 0 ||
                                     currentChannel.m_nMediaType.Contains(0) ||
                                     // or if at least one of the media types of the channel exists in the request
                                     typeIntersection.Count() > 0)
                                 {
                                     UnifiedSearchDefinitions definitions = CatalogLogic.BuildInternalChannelSearchObjectWithBaseRequest(
                                         currentChannel,
                                         request,
                                         groupInCache,
                                         groupId,
                                         doesGroupUsesTemplates,
                                         isAllowedToViewInactiveAssets,
                                         assetFilterKsql);

                                     // If specific types were requested
                                     if (mediaTypes.Length > 0 && !mediaTypes.Contains("0"))
                                     {
                                         // if channel has specific types defined
                                         if (currentChannel.m_nMediaType != null && currentChannel.m_nMediaType.Count > 0 &&
                                             !currentChannel.m_nMediaType.Contains(0))
                                         {
                                             // Search request will be the intersection of the request types and the channel types
                                             definitions.mediaTypes.AddRange(typeIntersection.Select(t => int.Parse(t)));
                                         }
                                         else
                                         // if specific types were requested but channel is oblivious to this - use the request types
                                         {
                                             definitions.mediaTypes.AddRange(mediaTypes.Select(t => int.Parse(t)));
                                         }
                                     }

                                     searchObjectsArray[channelIndex] = definitions;
                                 }
                             }
                         }
                         catch (Exception ex)
                         {
                             log.Error(ex.Message, ex);
                         }
                     }, searchObjectIndex);
                channelsSearchObjectTasks[searchObjectIndex].Start();
            }

            //Wait for all parallel tasks to end
            Task.WaitAll(channelsSearchObjectTasks);

            searchObjectsList = searchObjectsArray.ToList();

            // Dispose task objects
            for (int i = 0; i < channelsSearchObjectTasks.Length; i++)
            {
                if (channelsSearchObjectTasks[i] != null)
                {
                    channelsSearchObjectTasks[i].Dispose();
                }
            }

            // BEO-4996: Remove all empty search objects, because of asset type filtering...
            searchObjectsList.RemoveAll(o => o == null);

            return searchObjectsList;
        }
    }
}
