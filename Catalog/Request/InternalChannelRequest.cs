using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog.Request
{
    [Serializable]
    [DataContract]
    public class InternalChannelRequest : UnifiedChannelRequest
    {
        #region Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public ApiObjects.SearchObjects.OrderObj order;

        #endregion

        #region Ctor

        public InternalChannelRequest(string innerChannelId, int groupID,
            int pageSize, int pageIndex, string userIP, string signature, string signString, Filter filter, string filterQuery, ApiObjects.SearchObjects.OrderObj order)
            : base(groupID, pageSize, pageIndex, userIP, signature, signString, filter, filterQuery, innerChannelId)
        {
            this.filterQuery = filterQuery;
            this.order = order;
        }

        #endregion

        protected override ApiObjects.Response.Status GetAssets(UnifiedChannelRequest request, out int totalItems, out List<Response.UnifiedSearchResult> searchResults)
        {
            InternalChannelRequest internalRequest = request as InternalChannelRequest;

            if (internalRequest == null)
            {
                internalRequest = new InternalChannelRequest(this.channelID, this.m_nGroupID, this.m_nPageSize, this.m_nPageIndex,
                    this.m_sUserIP, this.m_sSignature, this.m_sSignString, this.m_oFilter, this.filterQuery, this.order);
            }

            return Catalog.GetInternalChannelAssets(internalRequest, out totalItems, out searchResults);
        }
    }
}
