using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using TVinciShared;
using Tvinci.Core.DAL;
using System.Threading.Tasks;
using ApiObjects.SearchObjects;
using System.Collections.Concurrent;
using Core.Catalog.Cache;
using GroupsCacheManager;
using Core.Catalog.Response;
using KLogMonitor;
using KlogMonitorHelper;
using ApiObjects.Response;
using ApiObjects.Catalog;

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

                // Get group from cache
                GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                Group groupInCache = groupManager.GetGroup(nParentGroupID);

                if (groupInCache != null)
                {
                    // Get channel IDs of current bundle
                    List<int> channelIds = CatalogLogic.GetBundleChannelIds(groupInCache.m_nParentGroupID, request.m_nBundleID, request.m_eBundleType);
                    List<GroupsCacheManager.Channel> allChannels = groupManager.GetChannels(channelIds, groupInCache.m_nParentGroupID);

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

                            int[] deviceRuleIds = null;

                            if (request.m_oFilter != null)
                            {
                                deviceRuleIds = ProtocolsFuncs.GetDeviceAllowedRuleIDs(request.m_oFilter.m_sDeviceId, request.m_nGroupID).ToArray();
                            }

                            List<BaseSearchObject> searchObjectsList = 
                                BuildBaseSearchObjects(request, groupInCache, allChannels, sMediaTypesFromRequest, deviceRuleIds, request.m_oOrderObj);

                            if (searchObjectsList != null && searchObjectsList.Count > 0)
                            {
                                try
                                {
                                    ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();
                                    if (searcher != null)
                                    {
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
                                            searcher.SearchSubscriptionAssets(request.m_nGroupID, 
                                                searchObjectsList, request.m_oFilter.m_nLanguage, request.m_oFilter.m_bUseStartDate, 
                                                request.m_sMediaType, oSearchOrder, request.m_nPageIndex, request.m_nPageSize, ref totalItems);

                                        if (searchResults != null)
                                        {
                                            response.m_nTotalItems = totalItems;
                                            response.searchResults = searchResults;
                                        }
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

        public static List<BaseSearchObject> BuildBaseSearchObjects(BaseRequest request, Group groupInCache, 
            List<GroupsCacheManager.Channel> allChannels, string[] mediaTypes, int[] deviceRuleIds, OrderObj order)
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
            ContextData contextData = new ContextData();

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

                             if (groupInCache != null)
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
                                     if (currentChannel.m_nChannelTypeID == (int)ChannelType.KSQL)
                                     {
                                         UnifiedSearchDefinitions definitions = CatalogLogic.BuildInternalChannelSearchObjectWithBaseRequest(currentChannel, request, groupInCache);

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
                                     else
                                     {
                                         MediaSearchObj channelSearchObject = CatalogLogic.BuildBaseChannelSearchObject(currentChannel, request,
                                             order, request.m_nGroupID, groupInCache.m_sPermittedWatchRules, deviceRuleIds, groupInCache.GetGroupDefaultLanguage());

                                         if ((currentChannel.m_nMediaType.Contains(0)) &&
                                             !(mediaTypes.Contains<string>("0")) && mediaTypes.Length > 0)
                                         {
                                             channelSearchObject.m_sMediaTypes =
                                                 string.Join(";", typeIntersection);
                                         }
                                         channelSearchObject.m_oOrder.m_eOrderBy = OrderBy.ID;
                                         searchObjectsArray[channelIndex] = channelSearchObject;
                                     }
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

            return searchObjectsList;
        }
    }
}
