using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    /// <summary>
    /// A search request of several types of assets: Media, EPGs etc. All in one, unified place.
    /// </summary>
    [DataContract]
    public class UnifiedSearchRequest : BaseRequest, IRequestImp
    {
        #region Data Members

        [DataMember]
        public bool isExact;
        
        [DataMember]
        public OrderObj order;
        
        [DataMember]
        public List<KeyValue> andList;
        
        [DataMember]
        public List<KeyValue> orList;

        [DataMember]
        public UnifiedQueryType queryType;

        #endregion

        #region Ctor

        /// <summary>
        /// Regulat constructor that initializes the request members
        /// </summary>
        /// <param name="nPageSize"></param>
        /// <param name="nPageIndex"></param>
        /// <param name="nGroupID"></param>
        /// <param name="sSignature"></param>
        /// <param name="sSignString"></param>
        /// <param name="isExact"></param>
        /// <param name="order"></param>
        /// <param name="searchValue"></param>
        /// <param name="ands"></param>
        /// <param name="ors"></param>
        /// <param name="type"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public UnifiedSearchRequest(int nPageSize, int nPageIndex, int nGroupID, string sSignature, string sSignString,
            bool isExact, OrderObj order, string searchValue, List<KeyValue> ands, List<KeyValue> ors, UnifiedQueryType type)
                : base(nPageSize, nPageIndex, string.Empty, nGroupID, null, sSignature, sSignString)
        {
            this.isExact = isExact;
            this.order = order;
            this.queryType = type;
            this.andList = ands;
            this.orList = ors;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Perform the unified search and return the Ids of the assets and their types
        /// </summary>
        /// <param name="baseRequest"></param>
        /// <returns></returns>
        public BaseResponse GetResponse(BaseRequest baseRequest)
        {
            try
            {
                UnifiedSearchRequest request = baseRequest as UnifiedSearchRequest;
                UnifiedSearchResponse response = new UnifiedSearchResponse();

                if (request == null)
                    throw new ArgumentNullException("request object is null or Required variables is null");

                CheckSignature(baseRequest);

                int nTotalItems = 0;
                List<UnifiedSearchResult> assetsResults = Catalog.GetAssetIdFromSearcher(request, ref nTotalItems);

                response.m_nTotalItems = nTotalItems;

                if (nTotalItems > 0)
                {
                    response.searchResults = assetsResults;
                }

                return (BaseResponse)response;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error - GetResponse", string.Format("Exception: message = {0}, ST = {1}", ex.Message, ex.StackTrace), this.GetType().Name);
                throw ex;
            }
        }

        #endregion
    }
}