using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.SearchObjects;
using Logger;
using TVinciShared;

namespace Catalog
{
    [DataContract]
    public class ChannelsContainingMediaRequest : BaseRequest, IRequestImp
    {
        protected static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DataMember]
        public List<int> m_lChannles;
        [DataMember]
        public int m_nMediaID;

        public ChannelsContainingMediaRequest()
            : base()
        {
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {            
            ChannelsContainingMediaResponse response = new ChannelsContainingMediaResponse();
            try
            {
                ChannelsContainingMediaRequest request = (ChannelsContainingMediaRequest)oBaseRequest;

                if (request == null || request.m_lChannles == null || request.m_lChannles.Count == 0)
                    throw new Exception("request object is null or Required variables is null");

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);

                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures dosen't match");

                //IF ElasticSearch 
                ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();
                if (searcher != null)
                {
                    if (searcher is ElasticsearchWrapper)
                    {
                        List<int> nChannels = searcher.GetMediaChannels(request.m_nGroupID, request.m_nMediaID);

                        if (nChannels != null && nChannels.Count > 0)
                        {
                            Dictionary<int, int> dChannels = nChannels.ToDictionary<int, int>(item => item);

                            foreach (int item in request.m_lChannles)
                            {   
                                if (dChannels.ContainsKey(item))
                                {
                                    response.m_lChannellList.Add(item);
                                }

                            }
                        }
                    }
                    else //LuceneWrapper
                    {
                        Group groupInCache = GroupsCache.Instance.GetGroup(request.m_nGroupID);
                        List<int> channelIds = request.m_lChannles;

                        if (groupInCache != null && channelIds != null && channelIds.Count > 0)
                        {
                            // Buils search Object per channelId call Searcher to return true/false result
                            List<Channel> allChannels = GroupsCache.Instance.GetChannelsFromCache(channelIds, request.m_nGroupID);

                            //    Build search object per channel
                            if (allChannels != null && allChannels.Count > 0)
                            {
                                List<ChannelContainSearchObj> channelsSearchObjects = new List<ChannelContainSearchObj>();
                                List<int> lIds = new List<int>();
                                Task[] channelsSearchObjectTasks = new Task[allChannels.Count];

                                #region Building search object for each channel
                                OrderObj oOrderObj = new OrderObj();
                                oOrderObj.m_eOrderBy = OrderBy.RELATED;

                                int[] nDeviceRuleId = null;
                                //get media DeviceRuleID
                                string sDeviceRuleID = string.Empty;
                                DataTable dt = DAL.ApiDAL.Get_DeviceMediaRules(request.m_nMediaID, request.m_nGroupID, null);
                                if (dt != null && dt.DefaultView.Count > 0)
                                    sDeviceRuleID = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["device_rule_id"]);

                                if (!string.IsNullOrEmpty(sDeviceRuleID))
                                    nDeviceRuleId = ProtocolsFuncs.GetDeviceAllowedRuleIDs(sDeviceRuleID, request.m_nGroupID).ToArray();

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
                                                     MediaSearchObj channelSearchObject = Catalog.BuildBaseChannelSearchObject(currentChannel, request, oOrderObj, groupInCache.m_nParentGroupID, groupInCache.m_sPermittedWatchRules, nDeviceRuleId);
                                                     if (channelSearchObject != null)
                                                     {
                                                         channelSearchObject.m_nMediaID = request.m_nMediaID;
                                                     }
                                                     ChannelContainSearchObj oCCSearchObj = new ChannelContainSearchObj();
                                                     oCCSearchObj.m_nChannelID = currentChannel.m_nChannelID;
                                                     oCCSearchObj.m_oSearchObj = channelSearchObject;
                                                     channelsSearchObjects.Add(oCCSearchObj);
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
                                #endregion

                                #region Search
                                if (channelsSearchObjects != null && channelsSearchObjects.Count > 0)
                                {
                                    try
                                    {
                                        // Getting true/false result if media exists in at least one channel
                                        List<ChannelContainObj> oChannelsContain = searcher.GetSubscriptionContainingMedia(channelsSearchObjects);

                                        //update the right places with true/ false                          

                                        foreach (ChannelContainObj item in oChannelsContain)
                                        {
                                            if (item.m_bContain)
                                                response.m_lChannellList.Add(item.m_nChannelID);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(ex.Message);
                                    }                                   
                                }
                                #endregion
                            }
                        }
                    }
                }
                return (BaseResponse)response;
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("AllChannelsContainingMediaRequest failed ex={0} ", ex.Message);
                return null;
            }
        }
    }
}