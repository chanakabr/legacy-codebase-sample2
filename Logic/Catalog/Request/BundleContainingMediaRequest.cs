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
using ApiObjects.Catalog;

namespace Core.Catalog.Request
{
    [DataContract]
    public class BundleContainingMediaRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public eBundleType m_eBundleType;
        [DataMember]
        public int m_nBundleID;
        [DataMember]
        public int m_nMediaID;
        [DataMember]
        public string m_sMediaType;

        public BundleContainingMediaRequest()
            : base()
        {
        }

        private void CheckRequestValidness(BundleContainingMediaRequest request)
        {
            if (request == null || request.m_nBundleID == 0)
                throw new Exception("Request object is null or Bundle ID is 0");
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                BundleContainingMediaRequest request = oBaseRequest as BundleContainingMediaRequest;
                CheckRequestValidness(request);
                CheckSignature(request);

                List<SearchResult> lMedias = new List<SearchResult>();
                ContainingMediaResponse response = new ContainingMediaResponse();

                GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                Group groupInCache = groupManager.GetGroup(nParentGroupID);

                if (groupInCache == null)
                {
                    log.Error("Could not load group cache");
                    return response;
                }

                if (groupInCache.m_nParentGroupID != request.m_nGroupID)
                    throw new Exception("BundleID does not belong to group");


                response.m_bContainsMedia = false;
                response.m_nTotalItems = 0;

                List<int> channelIds = CatalogLogic.GetBundleChannelIds(request.m_nGroupID, request.m_nBundleID, request.m_eBundleType);
                List<GroupsCacheManager.Channel> allChannels = groupManager.GetChannels(channelIds, groupInCache.m_nParentGroupID);


                if (channelIds != null && channelIds.Count > 0 && allChannels != null && allChannels.Count > 0)
                {
                    ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

                    #region searcher is LuceneWrapper
                    if (searcher is LuceneWrapper)
                    {
                        List<ApiObjects.SearchObjects.MediaSearchObj> channelsSearchObjects = new List<ApiObjects.SearchObjects.MediaSearchObj>();

                        // save monitor and logs context data
                        ContextData contextData = new ContextData();

                        Task[] channelsSearchObjectTasks = new Task[allChannels.Count];
                        int[] nDeviceRuleId = null;

                        if (request.m_oFilter != null)
                            nDeviceRuleId = ProtocolsFuncs.GetDeviceAllowedRuleIDs(request.m_oFilter.m_sDeviceId, request.m_nGroupID).ToArray();
                        #region Building search object for each channel
                        for (int searchObjectIndex = 0; searchObjectIndex < allChannels.Count; searchObjectIndex++)
                        {
                            channelsSearchObjectTasks[searchObjectIndex] = new Task(
                                 (obj) =>
                                 {
                                     // load monitor and logs context data
                                     contextData.Load();

                                     try
                                     {
                                         if (groupInCache != null)
                                         {
                                             GroupsCacheManager.Channel currentChannel = allChannels[(int)obj];
                                             ApiObjects.SearchObjects.MediaSearchObj channelSearchObject = CatalogLogic.BuildBaseChannelSearchObject(currentChannel, request, null, groupInCache.m_nParentGroupID, groupInCache.m_sPermittedWatchRules, nDeviceRuleId, groupInCache.GetGroupDefaultLanguage());
                                             channelSearchObject.m_oOrder.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.ID;
                                             channelsSearchObjects.Add(channelSearchObject);
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
                        for (int i = 0; i < channelsSearchObjectTasks.Length; i++)
                        {
                            if (channelsSearchObjectTasks[i] != null)
                            {
                                channelsSearchObjectTasks[i].Dispose();
                            }
                        }
                        #endregion

                        if (channelsSearchObjects != null && channelsSearchObjects.Count > 0)
                        {
                            try
                            {
                                if (searcher != null)
                                {
                                    // Getting all medias in Bundle
                                    SearchResultsObj oSearchResult = null;
                                        //searcher.SearchSubscriptionMedias(request.m_nGroupID, channelsSearchObjects, 
                                        //request.m_oFilter.m_nLanguage, request.m_oFilter.m_bUseStartDate, 
                                        //request.m_sMediaType, new OrderObj(), request.m_nPageIndex, request.m_nPageSize);

                                    if (oSearchResult != null && oSearchResult.m_resultIDs != null && oSearchResult.m_resultIDs.Count > 0)
                                    {
                                        bool exists = oSearchResult.m_resultIDs.Select(searchRes => searchRes.assetID).Any(searchMediaID => searchMediaID == m_nMediaID);

                                        if (exists)
                                        {
                                            response.m_nTotalItems = 1;
                                            response.m_bContainsMedia = true;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex.Message);
                            }
                        }
                    }
                    #endregion
                    #region searcher is ElasticSearchWrapper
                    else
                    {
                        List<int> lChannelIDs = allChannels.Select(channel => channel.m_nChannelID).ToList();
                        bool bDoesMediaBelongToBundle = searcher.DoesMediaBelongToChannels(groupInCache.m_nParentGroupID, lChannelIDs, request.m_nMediaID);

                        if (bDoesMediaBelongToBundle)
                        {
                            response.m_bContainsMedia = true;
                            response.m_nTotalItems = 1;
                        }
                        else
                        {
                            response.m_bContainsMedia = false;
                            response.m_nTotalItems = 0;
                        }
                    }
                    #endregion
                }
                return (BaseResponse)response;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
