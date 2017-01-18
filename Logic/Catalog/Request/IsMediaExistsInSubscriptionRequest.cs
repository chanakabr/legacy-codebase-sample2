using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.SearchObjects;
using Core.Catalog.Cache;
using Core.Catalog.Response;
using GroupsCacheManager;
using KLogMonitor;
using KlogMonitorHelper;
using TVinciShared;
using ApiObjects.Catalog;

namespace Core.Catalog.Request
{
    [DataContract]
    public class IsMediaExistsInSubscriptionRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public int m_nSubscriptionID;
        [DataMember]
        public int m_nMediaID;

        public IsMediaExistsInSubscriptionRequest()
            : base()
        {
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                IsMediaExistsInSubscriptionRequest request = oBaseRequest as IsMediaExistsInSubscriptionRequest;
                List<SearchResult> lMedias = new List<SearchResult>();

                IsMediaExistsInSubscriptionResponse response = new IsMediaExistsInSubscriptionResponse();

                if (request == null || request.m_nSubscriptionID == 0)
                    throw new ArgumentException("request object is null or Required variables is null");

                CheckSignature(request);

                response.m_bExists = false;
                response.m_nTotalItems = 0;

                GroupManager groupManager = new GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                Group groupInCache = groupManager.GetGroup(nParentGroupID);

                List<int> channelIds = CatalogLogic.GetBundleChannelIds(request.m_nGroupID, request.m_nSubscriptionID, eBundleType.SUBSCRIPTION);
                if (groupInCache != null && channelIds != null && channelIds.Count > 0)
                {
                    // Builds search Object per channelId call Searcher to return true/false result
                    List<GroupsCacheManager.Channel> allChannels = groupManager.GetChannels(channelIds, groupInCache.m_nParentGroupID);


                    if (allChannels != null && allChannels.Count > 0)
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
                            #endregion

                            if (channelsSearchObjects != null && channelsSearchObjects.Count > 0)
                            {
                                try
                                {
                                    if (searcher != null)
                                    {

                                        // Getting all medias in subscription
                                        SearchResultsObj oSearchResult = null;
                                            //searcher.SearchSubscriptionMedias(request.m_nGroupID, channelsSearchObjects, request.m_oFilter.m_nLanguage, request.m_oFilter.m_bUseStartDate, string.Empty, new OrderObj(), request.m_nPageIndex, request.m_nPageSize);

                                        if (oSearchResult != null && oSearchResult.m_resultIDs != null && oSearchResult.m_resultIDs.Count > 0)
                                        {

                                            IList<int> mediaIDsInList = oSearchResult.m_resultIDs.Select(searchRes => searchRes.assetID).Where(searchMediaID => searchMediaID == m_nMediaID).ToList();
                                            if (mediaIDsInList.Count > 0)
                                            {
                                                response.m_nTotalItems = 1;
                                                response.m_bExists = true;
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
                            int nOwnerGroup = groupInCache.m_nParentGroupID;

                            bool bDoesMediaBelongToSubscription = searcher.DoesMediaBelongToChannels(nOwnerGroup, lChannelIDs, request.m_nMediaID);

                            if (bDoesMediaBelongToSubscription)
                            {
                                response.m_bExists = true;
                                response.m_nTotalItems = 1;
                            }
                        }
                        #endregion
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
