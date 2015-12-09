using Catalog.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects.SearchObjects;
using ApiObjects.Response;
using System.Web;

namespace Catalog.Request
{
    [Serializable]
    [DataContract]
    public class ExternalChannelRequest : UnifiedChannelRequest
    {
        #region Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public string deviceId;

        [DataMember]
        public string deviceType;

        [DataMember]
        public string utcOffset;

        #endregion

        #region Ctor

        public ExternalChannelRequest(string channelId, string externalIdentifier, int groupID,
            int pageSize, int pageIndex, string userIP, string signature, string signString, Filter filter, string deviceId, string deviceType, string filterQuery = "")
            : base(groupID, pageSize, pageIndex, userIP, signature, signString, filter, filterQuery, channelId, externalIdentifier)
        {
            this.deviceId = deviceId;
            this.deviceType = deviceType;
        }

        #endregion

        #region Override Methods

        protected override Status GetAssets(UnifiedChannelRequest request, out int totalItems, out List<UnifiedSearchResult> searchResults, out string requestId)
        {
            ExternalChannelRequest externalRequest = request as ExternalChannelRequest;

            if (externalRequest == null)
            {
                externalRequest = new ExternalChannelRequest(this.internalChannelID, this.externalChannelID, this.m_nGroupID, this.m_nPageSize, this.m_nPageIndex,
                    this.m_sUserIP, this.m_sSignature, this.m_sSignString, this.m_oFilter, this.deviceId, this.deviceType);
            }

            return Catalog.GetExternalChannelAssets(externalRequest, out totalItems, out searchResults, out requestId);
        }

        #endregion

    }
}
