using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Request
{
    [Serializable]
    [DataContract]
    public class InternalChannelRequest : BaseChannelRequest
    {
        #region Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public ApiObjects.SearchObjects.OrderObj order;
        [DataMember]
        public bool m_bIgnoreDeviceRuleID;

        #endregion

        #region Ctor

        public InternalChannelRequest()
            : base()
        {
        }

        public InternalChannelRequest(string channelId, string externalIdentifier, int groupID,
            int pageSize, int pageIndex, string userIP, string signature, string signString, Filter filter, string filterQuery, ApiObjects.SearchObjects.OrderObj order)
            : base(groupID, pageSize, pageIndex, userIP, signature, signString, filter, filterQuery, channelId, externalIdentifier)
        {
            this.filterQuery = filterQuery;
            this.order = order;
            m_bIgnoreDeviceRuleID = false;
        }

        #endregion

        protected override ApiObjects.Response.Status GetAssets(BaseChannelRequest request, out int totalItems, out List<Response.UnifiedSearchResult> searchResults)
        {
            InternalChannelRequest internalRequest = request as InternalChannelRequest;
            
            if (internalRequest == null)
            {
                internalRequest = new InternalChannelRequest(this.internalChannelID, this.externalChannelID, this.m_nGroupID, this.m_nPageSize, this.m_nPageIndex,
                    this.m_sUserIP, this.m_sSignature, this.m_sSignString, this.m_oFilter, this.filterQuery, this.order);
            }

            return CatalogLogic.GetInternalChannelAssets(internalRequest, out totalItems, out searchResults);
        }
    }
}
