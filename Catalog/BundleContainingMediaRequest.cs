using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Logger;
using System.Reflection;
using System.Data;
using TVinciShared;
using Tvinci.Core.DAL;
using System.Threading.Tasks;
using ApiObjects.SearchObjects;
using System.Collections.Concurrent;

namespace Catalog
{
    [DataContract]
    public class BundleContainingMediaRequest : BaseRequest, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                BundleContainingMediaRequest request = oBaseRequest as BundleContainingMediaRequest;
                CheckRequestValidness(request);
                CheckSignature(request);

                List<SearchResult> lMedias = new List<SearchResult>();
                ContainingMediaResponse response = new ContainingMediaResponse();

                Group groupInCache = GroupsCache.Instance.GetGroup(request.m_nGroupID);
                if (groupInCache == null)
                {
                    _logger.Error("Could not load group cache");
                    return response;
                }

                if (groupInCache.m_nParentGroupID != request.m_nGroupID)
                    throw new Exception("BundleID does not belong to group");


                response.m_bContainsMedia = false;
                response.m_nTotalItems = 0;

                List<int> channelIds = Catalog.GetBundleChannelIds(request.m_nGroupID, request.m_nBundleID, request.m_eBundleType);
                List<Channel> allChannels = GroupsCache.Instance.GetChannelsFromCache(channelIds, request.m_nGroupID);

                if (channelIds != null && channelIds.Count > 0 && allChannels != null && allChannels.Count > 0)
                {
                    ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

                    #region searcher is LuceneWrapper
                    if (searcher is LuceneWrapper)
                    {
                        List<ApiObjects.SearchObjects.MediaSearchObj> channelsSearchObjects = new List<ApiObjects.SearchObjects.MediaSearchObj>();

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
                                     try
                                     {
                                         if (groupInCache != null)
                                         {
                                             Channel currentChannel = allChannels[(int)obj];
                                             ApiObjects.SearchObjects.MediaSearchObj channelSearchObject = Catalog.BuildBaseChannelSearchObject(currentChannel, request, null, groupInCache.m_nParentGroupID, groupInCache.m_sPermittedWatchRules, nDeviceRuleId, groupInCache.GetGroupDefaultLanguage());
                                             channelSearchObject.m_oOrder.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.ID;
                                             channelsSearchObjects.Add(channelSearchObject);
                                         }
                                     }
                                     catch (Exception ex)
                                     {
                                         _logger.Error(ex.Message, ex);
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
                                    SearchResultsObj oSearchResult = searcher.SearchSubscriptionMedias(request.m_nGroupID, channelsSearchObjects, request.m_oFilter.m_nLanguage, request.m_oFilter.m_bUseStartDate, request.m_sMediaType, new OrderObj(), request.m_nPageIndex, request.m_nPageSize);

                                    if (oSearchResult != null && oSearchResult.m_resultIDs != null && oSearchResult.m_resultIDs.Count > 0)
                                    {

                                        IList<int> mediaIDsInList = oSearchResult.m_resultIDs.Select(searchRes => searchRes.assetID).Where(searchMediaID => searchMediaID == m_nMediaID).ToList();
                                        if (mediaIDsInList.Count > 0)
                                        {
                                            response.m_nTotalItems = 1;
                                            response.m_bContainsMedia = true;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex.Message);
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
                _logger.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
