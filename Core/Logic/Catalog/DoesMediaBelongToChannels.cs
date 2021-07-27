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
using Core.Catalog.Cache;
using GroupsCacheManager;
using Core.Catalog.Request;
using Core.Catalog.Response;
using KLogMonitor;
using KlogMonitorHelper;

namespace Core.Catalog
{
    [DataContract]
    /*  Get Channels List + media ID and return true / false value */
    public class DoesMediaBelongToChannels : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public List<int> m_lChannelIDs;
        [DataMember]
        public int m_nMediaID;

        public DoesMediaBelongToChannels()
            : base()
        {
        }

        public DoesMediaBelongToChannels(DoesMediaBelongToChannels c)
            : base(c.m_nPageSize, c.m_nPageIndex, c.m_sUserIP, c.m_nGroupID, c.m_oFilter, c.m_sSignature, c.m_sSignString)
        {
            m_lChannelIDs = c.m_lChannelIDs;
            m_nMediaID = c.m_nMediaID;
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                DoesMediaBelongToChannels request = oBaseRequest as DoesMediaBelongToChannels;
                List<SearchResult> lMedias = new List<SearchResult>();

                ContainingMediaResponse response = new ContainingMediaResponse();

                if (request == null || request.m_lChannelIDs == null || request.m_lChannelIDs.Count == 0)
                    throw new ArgumentException("request object is null or Required variables is null");

                CheckSignature(request);

                IIndexManager indexManager = IndexManagerFactory.GetInstance(m_nGroupID);

                bool bDoesMediaBelongToSubscription = indexManager.DoesMediaBelongToChannels(request.m_lChannelIDs, request.m_nMediaID);
                if (bDoesMediaBelongToSubscription)
                {
                    response.m_bContainsMedia = true;
                    response.m_nTotalItems = 1;
                }
                else
                {
                    response.m_bContainsMedia = false;
                    response.m_nTotalItems = 0;
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
