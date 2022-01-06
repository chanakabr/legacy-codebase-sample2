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
using Phx.Lib.Log;

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
                        IIndexManager indexManager = IndexManagerFactory.Instance.GetIndexManager(groupInCache.m_nParentGroupID);
                        List<int> lChannelIDs = allChannels.Select(channel => channel.m_nChannelID).ToList();
                        
                        bool bDoesMediaBelongToSubscription = indexManager.DoesMediaBelongToChannels(lChannelIDs, request.m_nMediaID);

                        if (bDoesMediaBelongToSubscription)
                        {
                            response.m_bExists = true;
                            response.m_nTotalItems = 1;
                        }
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
