using ApiObjects.SearchObjects;
using Catalog.Response;
using ElasticSearch.Searcher;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects.SearchPriorityGroups;
using Core.Catalog.Request.SearchPriority;

namespace Core.Catalog.Request
{
    [Serializable]
    [DataContract]
    public class InternalChannelRequest : BaseChannelRequest, ISearchPriorityRequest
    {
        #region Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public OrderObj order;

        [DataMember]
        public IReadOnlyCollection<AssetOrder> orderingParameters;
        
        [DataMember]
        public bool m_bIgnoreDeviceRuleID;
        
        [DataMember]
        public bool isAllowedToViewInactiveAssets;

        [DataMember]
        public SearchAggregationGroupBy searchGroupBy;
        
        /// <summary>
        /// Key Value Pair. Key - Score. Value - Corresponding Priority Group.
        /// </summary>
        public IReadOnlyDictionary<double, SearchPriorityGroup> PriorityGroupsMappings { get; set; }

        #endregion

        #region Ctor

        public InternalChannelRequest()
            : base()
        {
        }

        public InternalChannelRequest(
            string channelId,
            string externalIdentifier,
            int groupID,
            int pageSize,
            int pageIndex,
            string userIP,
            string signature,
            string signString,
            Filter filter,
            string filterQuery,
            OrderObj order,
            long? originalUserId)
            : base(groupID, pageSize, pageIndex, userIP, signature, signString, filter, filterQuery, channelId, externalIdentifier, originalUserId)
        {
            this.filterQuery = filterQuery;
            this.order = order;
            m_bIgnoreDeviceRuleID = false;
        }

        #endregion

        protected override ApiObjects.Response.Status GetAssets(BaseChannelRequest request, out int totalItems, out List<Response.UnifiedSearchResult> searchResults, out List<AggregationsResult> aggregationsResult)
        {
            InternalChannelRequest internalRequest = request as InternalChannelRequest;
            
            if (internalRequest == null)
            {
                internalRequest = new InternalChannelRequest(this.internalChannelID, this.externalChannelID, this.m_nGroupID, this.m_nPageSize, this.m_nPageIndex,
                    this.m_sUserIP, this.m_sSignature, this.m_sSignString, this.m_oFilter, this.filterQuery, this.order, OriginalUserId);
            }

            if (request.m_dServerTime == default(DateTime) || request.m_dServerTime == DateTime.MinValue)
            {
                request.m_dServerTime = DateTime.UtcNow;
            }

            return CatalogLogic.GetInternalChannelAssets(internalRequest, out totalItems, out searchResults, out aggregationsResult);

        }

        internal virtual bool GetShouldUseSearchEndDate()
        {
            return ConditionalAccess.Utils.GetIsTimeShiftedTvPartnerSettingsExists(m_nGroupID);
        }

    }
}
