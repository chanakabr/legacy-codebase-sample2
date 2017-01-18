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
    public class BundleMediaRequest : BaseRequest, IRequestImp
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

        public BundleMediaRequest()
            : base()
        {
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                BundleMediaRequest request = (BundleMediaRequest)oBaseRequest;
                List<SearchResult> lMedias = new List<SearchResult>();
                MediaIdsResponse response = new MediaIdsResponse();

                if (request == null || request.m_nBundleID == 0)
                    throw new Exception("request object is null or Required variables is null");

                CheckSignature(request);

                string dataTable = string.Empty;
                switch (request.m_eBundleType)
                {
                    case eBundleType.SUBSCRIPTION:
                    {
                        dataTable = SUB_DATA_TABLE;
                        break;
                    }
                    case eBundleType.COLLECTION:
                    {
                        dataTable = COL_DATA_TABLE;
                        break;
                    }
                }

                GroupsCacheManager.GroupManager groupManager = new GroupsCacheManager.GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                Group groupInCache = groupManager.GetGroup(nParentGroupID);

                if (groupInCache != null)
                {
                    List<int> channelIds = CatalogLogic.GetBundleChannelIds(groupInCache.m_nParentGroupID, request.m_nBundleID, request.m_eBundleType);
                    List<GroupsCacheManager.Channel> allChannels = groupManager.GetChannels(channelIds, groupInCache.m_nParentGroupID);



                    if (channelIds != null && channelIds.Count > 0)
                    {
                        if (allChannels.Count > 0)
                        {
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

                            // save monitor and logs context data
                            ContextData contextData = new ContextData();

                            Task[] channelsSearchObjectTasks = new Task[allChannels.Count];

                            int[] nDeviceRuleId = null;
                            if (request.m_oFilter != null)
                                nDeviceRuleId = ProtocolsFuncs.GetDeviceAllowedRuleIDs(request.m_oFilter.m_sDeviceId, request.m_nGroupID).ToArray();

                            MediaSearchObj[] arrChannelSearchObjects = new MediaSearchObj[allChannels.Count];

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
                                             int nChannelIndex = (int)obj;

                                             if (groupInCache != null)
                                             {
                                                 GroupsCacheManager.Channel currentChannel = allChannels[nChannelIndex];

                                                 if (sMediaTypesFromRequest.Contains<string>("0") || sMediaTypesFromRequest.Contains<string>(currentChannel.m_nMediaType.ToString()) || currentChannel.m_nMediaType.ToString().Equals("0"))
                                                 {
                                                     MediaSearchObj channelSearchObject = CatalogLogic.BuildBaseChannelSearchObject(currentChannel, request, request.m_oOrderObj, request.m_nGroupID, groupInCache.m_sPermittedWatchRules, nDeviceRuleId, groupInCache.GetGroupDefaultLanguage());

                                                     if ((currentChannel.m_nMediaType.ToString().Equals("0") || string.IsNullOrEmpty(currentChannel.m_nMediaType.ToString())) && !(sMediaTypesFromRequest.Contains<string>("0")) && sMediaTypesFromRequest.Length > 0)
                                                     {
                                                         channelSearchObject.m_sMediaTypes = sMediaTypesFromRequest[0];
                                                     }
                                                     channelSearchObject.m_oOrder.m_eOrderBy = OrderBy.ID;
                                                     arrChannelSearchObjects[nChannelIndex] = channelSearchObject;
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

                            List<MediaSearchObj> channelsSearchObjects = arrChannelSearchObjects.ToList();

                            for (int i = 0; i < channelsSearchObjectTasks.Length; i++)
                            {
                                if (channelsSearchObjectTasks[i] != null)
                                {
                                    channelsSearchObjectTasks[i].Dispose();
                                }
                            }

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
                                        CatalogLogic.GetOrderValues(ref oSearchOrder, request.m_oOrderObj);
                                        if (oSearchOrder.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.META && string.IsNullOrEmpty(oSearchOrder.m_sOrderValue))
                                        {
                                            oSearchOrder.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.CREATE_DATE;
                                            oSearchOrder.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                                        }


                                        // Getting all medias in bundle   
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
    }
}
