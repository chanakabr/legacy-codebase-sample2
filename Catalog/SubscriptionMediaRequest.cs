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
    public class SubscriptionMediaRequest : BaseRequest, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DataMember]
        public int m_nSubscriptionID;
        [DataMember]
        public OrderObj m_oOrderObj;
        [DataMember]
        public string m_sMediaType;

        public SubscriptionMediaRequest() 
            : base()
        {
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                SubscriptionMediaRequest request = (SubscriptionMediaRequest)oBaseRequest;
                List<SearchResult> lMedias = new List<SearchResult>();
                MediaIdsResponse response = new MediaIdsResponse();

                if (request == null || request.m_nSubscriptionID == 0)
                    throw new Exception("request object is null or Required variables is null");

                Int32 nOwnerGroup = int.Parse(ODBCWrapper.Utils.GetTableSingleVal("subscriptions", "group_id", request.m_nSubscriptionID, "pricing_connection").ToString());

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);

                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures dosen't match");

                Group groupInCache = GroupsCache.Instance.GetGroup(request.m_nGroupID);
                if (groupInCache != null)
                {
                    List<int> channelIds = Catalog.GetSubscriptionChannelIds(groupInCache.m_nParentGroupID, request.m_nSubscriptionID);
                    List<Channel> allChannels = GroupsCache.Instance.GetChannelsFromCache(channelIds, request.m_nGroupID);

                    if (channelIds != null && channelIds.Count > 0)
                    {
                        if (allChannels.Count > 0)
                        {
                            List<MediaSearchObj> channelsSearchObjects = new List<MediaSearchObj>();
                            string[] sMediaTypesFromRequest;
                            if (string.IsNullOrEmpty(request.m_sMediaType))
                                sMediaTypesFromRequest = new string[1] { "0" };
                            else
                            {
                                if (request.m_sMediaType.EndsWith(";"))
                                {
                                    request.m_sMediaType = request.m_sMediaType.Remove(request.m_sMediaType.Length - 1);
                                }

                                sMediaTypesFromRequest = request.m_sMediaType.Split(';');

                            }

                            //List<int> lIds = new List<int>();
                            Task[] channelsSearchObjectTasks = new Task[allChannels.Count];

                            int[] nDeviceRuleId = null;
                            if (request.m_oFilter != null)
                                nDeviceRuleId = ProtocolsFuncs.GetDeviceAllowedRuleIDs(request.m_oFilter.m_sDeviceId, request.m_nGroupID).ToArray();

                            // Building search object for each channel
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
                                                 if (sMediaTypesFromRequest.Contains<string>("0") || sMediaTypesFromRequest.Contains<string>(currentChannel.m_nMediaType.ToString()) || currentChannel.m_nMediaType.ToString().Equals("0"))
                                                 {
                                                     MediaSearchObj channelSearchObject = Catalog.BuildBaseChannelSearchObject(currentChannel, request, request.m_oOrderObj, nOwnerGroup, groupInCache.m_sPermittedWatchRules, nDeviceRuleId);

                                                     if ((currentChannel.m_nMediaType.ToString().Equals("0") || string.IsNullOrEmpty(currentChannel.m_nMediaType.ToString())) && !(sMediaTypesFromRequest.Contains<string>("0")) && sMediaTypesFromRequest.Length > 0)
                                                     {
                                                         channelSearchObject.m_sMediaTypes = sMediaTypesFromRequest[0];
                                                     }
                                                     channelSearchObject.m_oOrder.m_eOrderBy = OrderBy.ID;
                                                     channelsSearchObjects.Add(channelSearchObject);
                                                 }
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

                            if (channelsSearchObjects != null && channelsSearchObjects.Count > 0)
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
                                        Catalog.GetOrderValues(ref oSearchOrder, request.m_oOrderObj);
                                        if (oSearchOrder.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.META && string.IsNullOrEmpty(oSearchOrder.m_sOrderValue))
                                        {
                                            oSearchOrder.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.CREATE_DATE;
                                            oSearchOrder.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                                        }

                                        // Getting all medias in subscription   
                                        List<SearchResult> lMediaRes = null;
                                        SearchResultsObj oSearchResults = searcher.SearchSubscriptionMedias(request.m_nGroupID, channelsSearchObjects, request.m_oFilter.m_nLanguage, request.m_oFilter.m_bUseStartDate, request.m_sMediaType, oSearchOrder, request.m_nPageIndex, request.m_nPageSize);

                                        if (oSearchResults != null)
                                        {
                                            lMediaRes = Utils.GetMediaUpdateDate(oSearchResults.m_resultIDs);
                                        }

                                        if (lMediaRes != null && lMediaRes.Count > 0)
                                        {
                                            response.m_nMediaIds = new List<SearchResult>(lMediaRes);
                                            response.m_nTotalItems = oSearchResults.n_TotalItems;
                                        }
                                        else
                                        {
                                            response.m_nMediaIds = null;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(ex.Message);
                                }
                            }
                        }
                    }
                }

                Logger.Logger.Log("Info", "SubscriptionMediaRequest - total returned items = " + response.m_nTotalItems, "Elasticsearch");
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
