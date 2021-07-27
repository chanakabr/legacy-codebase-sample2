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
                    IIndexManager indexManager = IndexManagerFactory.GetInstance(groupInCache.m_nParentGroupID);
                    List<int> lChannelIDs = allChannels.Select(channel => channel.m_nChannelID).ToList();
                    bool bDoesMediaBelongToBundle = indexManager.DoesMediaBelongToChannels(lChannelIDs, request.m_nMediaID);

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
