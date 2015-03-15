using ApiObjects.Response;
using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;

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
        public List<string> assetTypes;

        [DataMember]
        public BooleanPhraseNode filterTree;

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
            bool isExact, OrderObj order, string searchValue,
            BooleanPhraseNode filterTree,
            List<string> types)
                : base(nPageSize, nPageIndex, string.Empty, nGroupID, null, sSignature, sSignString)
        {
            this.isExact = isExact;
            this.order = order;
            this.assetTypes = types;
            this.filterTree = filterTree;
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
            UnifiedSearchResponse response = new UnifiedSearchResponse();
            
            try
            {
                UnifiedSearchRequest request = baseRequest as UnifiedSearchRequest;

                if (request == null)
                {
                    throw new ArgumentNullException("request object is null or Required variables is null");
                }
                // Request is bad if there is no condition to query by; or the group is 0
                else if (filterTree == null ||
                       request.m_nGroupID == 0)
                {
                    response.status.Code = (int)eResponseStatus.BadSearchRequest;
                    response.status.Message = "Invalid request parameters";
                }
                else
                {

                    CheckSignature(baseRequest);

                    int totalItems = 0;
                    List<UnifiedSearchResult> assetsResults = Catalog.GetAssetIdFromSearcher(request, ref totalItems);

                    response.m_nTotalItems = totalItems;

                    if (totalItems > 0)
                    {
                        response.searchResults = assetsResults;
                    }

                    response.status.Code = (int)eResponseStatus.OK;
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error - GetResponse", string.Format("Exception: message = {0}, ST = {1}", ex.Message, ex.StackTrace), this.GetType().Name);

                if (ex is HttpException)
                {
                    if ((ex as HttpException).GetHttpCode() == 404)
                    {
                        response.status.Code = (int)eResponseStatus.IndexMissing;
                        response.status.Message = "Data not index for this group";
                    }
                    else
                    {
                        response.status.Code = (int)eResponseStatus.Error;
                        response.status.Message = "Got error with Elasticsearch";
                    }
                }
                else
                {
                    response.status.Code = (int)eResponseStatus.Error;
                    response.status.Message = "Search failed";
                }
            }

            return (BaseResponse)response;
        }

        #endregion
    }
}