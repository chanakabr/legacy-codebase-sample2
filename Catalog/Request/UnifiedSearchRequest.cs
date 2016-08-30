using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using Catalog.Response;
using KLogMonitor;
using System.Reflection;
using Catalog.Attributes;
using Catalog.Cache;

namespace Catalog.Request
{
    /// <summary>
    /// A search request of several types of assets: Media, EPGs etc. All in one, unified place.
    /// </summary>
    [LogTopic("UnifiedSearch")]
    [DataContract]
    public class UnifiedSearchRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Data Members

        [DataMember]
        public OrderObj order;

        [DataMember]
        public List<int> assetTypes;

        internal BooleanPhraseNode filterTree;

        [DataMember]
        public string filterQuery;

        [DataMember]
        public string nameAndDescription;

        [DataMember]
        public List<ePersonalFilter> personalFilters;

        [DataMember]
        public int from;

        [DataMember]
        public string requestId;

        [DataMember]
        public List<KeyValuePair<eAssetTypes, long>> specificAssets;

        [DataMember]
        public List<string> excludedCrids;

        /// <summary>
        /// Defines if start/end date KSQL search will be used only for EPG/recordings or for media as well
        /// </summary>
        [DataMember]
        public bool shouldDateSearchesApplyToAllTypes;

        //exectGroupId - add new GroupID to Search assets ONLY in specific group 
        [DataMember]
        public int exectGroupId;

        /// <summary>
        /// Defines if search will be used deviceRule
        /// </summary>
        [DataMember]
        public bool shouldIgnoreDeviceRuleID;


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
        /// <param name="order"></param>
        /// <param name="searchValue"></param>
        /// <param name="ands"></param>
        /// <param name="ors"></param>
        /// <param name="type"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public UnifiedSearchRequest(int nPageSize, int nPageIndex, int nGroupID, string sSignature, string sSignString,
            OrderObj order,
            List<int> types,
            string filterQuery,
            string nameAndDescription,
            BooleanPhraseNode filterTree = null)
            : base(nPageSize, nPageIndex, string.Empty, nGroupID, null, sSignature, sSignString)
        {
            this.order = order;
            this.assetTypes = types;
            this.filterTree = filterTree;
            this.filterQuery = filterQuery;
            this.nameAndDescription = nameAndDescription;
            this.shouldIgnoreDeviceRuleID = false;
            this.exectGroupId = 0;
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

                if (request.m_nGroupID == 0)
                {
                    var exception = new ArgumentException("No group Id was sent in request");
                    exception.Data["StatusCode"] = (int)eResponseStatus.BadSearchRequest;

                    throw exception;
                }

                BooleanPhraseNode filterTree = null;

                if (!string.IsNullOrEmpty(request.filterQuery))
                {
                    Status status = BooleanPhraseNode.ParseSearchExpression(filterQuery, ref filterTree);

                    if (status == null)
                    {
                        return new UnifiedSearchResponse()
                        {
                            status = new Status((int)eResponseStatus.SyntaxError, "Could not parse search expression")
                        };
                    }
                    else  if (status.Code != (int)eResponseStatus.OK)
                    {
                        return new UnifiedSearchResponse()
                        {
                            status = status
                        };
                    }
                }

                // If request asks for name and description filter
                if (string.IsNullOrEmpty(request.nameAndDescription))
                {
                    request.filterTree = filterTree;
                }
                else
                {
                    List<BooleanPhraseNode> newNodes = new List<BooleanPhraseNode>();
                    List<BooleanPhraseNode> nameAndDescriptionNodes = new List<BooleanPhraseNode>();

                    // "name = q OR description = q"
                    nameAndDescriptionNodes.Add(new BooleanLeaf("name", request.nameAndDescription, null, ComparisonOperator.Contains));
                    nameAndDescriptionNodes.Add(new BooleanLeaf("description", request.nameAndDescription, null, ComparisonOperator.Contains));

                    BooleanPhrase nameAndDescriptionPhrase = new BooleanPhrase(nameAndDescriptionNodes, eCutType.Or);

                    newNodes.Add(nameAndDescriptionPhrase);

                    // If there is no filter tree from the string, create a new one containing only name and description
                    // If there is a tree already, use it as a branch and connect it with "And" to "name and description" 
                    if (filterTree != null)
                    {
                        newNodes.Add(filterTree);
                    }

                    request.filterTree = new BooleanPhrase(newNodes, eCutType.And);
                }

                CheckSignature(baseRequest);

                // If this is a new request - generate a request ID based on the filter query
                if (string.IsNullOrEmpty(request.requestId))
                {
                    if (!string.IsNullOrEmpty(request.filterQuery))
                    {
                        request.requestId = request.filterQuery.Replace(' ', '_');
                    }
                    else
                    {
                        request.requestId = "empty_filter";
                    }
                }

                int totalItems = 0;
                int to = 0;
                List<UnifiedSearchResult> assetsResults = Catalog.GetAssetIdFromSearcher(request, ref totalItems, ref to);

                response.m_nTotalItems = totalItems;

                if (totalItems > 0)
                {
                    response.searchResults = assetsResults;
                }

                if (to > 0)
                {
                    response.to = to;
                }

                // Response request Id is identical to request's request Id
                response.requestId = request.requestId;

                response.status.Code = (int)eResponseStatus.OK;
            }
            catch (Exception ex)
            {
                log.Error("Error - GetResponse - " +
                    string.Format("Exception: group = {0} siteGuid = {1} filterPhrase = {2} message = {3}, ST = {4}",
                    baseRequest.m_nGroupID, // {0}
                    baseRequest.m_sSiteGuid, // {1}
                    // Use filter query if this is correct type
                    baseRequest is UnifiedSearchRequest ? (baseRequest as UnifiedSearchRequest).filterQuery : "", // {2}
                    ex.Message, // {3}
                    ex.StackTrace // {4}
                    ), ex);

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
                else if (ex is KalturaException)
                {
                    // This is a specific exception we created.
                    // If this specific KalturaException has StatusCode in its data, use it instead of the general code
                    if (ex.Data.Contains("StatusCode"))
                    {
                        response.status.Code = (int)ex.Data["StatusCode"];
                        response.status.Message = ex.Message;
                    }
                    else
                    {
                        response.status.Code = (int)eResponseStatus.Error;
                        response.status.Message = "Failed getting assets of channel";
                    }
                }
                else if (ex is ArgumentException)
                {
                    // This is a specific exception we created.
                    // If this specific ArgumentException has StatusCode in its data, use it instead of the general code
                    if (ex.Data.Contains("StatusCode"))
                    {
                        response.status.Code = (int)ex.Data["StatusCode"];
                        response.status.Message = ex.Message;
                    }
                    else
                    {
                        response.status.Code = (int)eResponseStatus.Error;
                        response.status.Message = "Failed getting assets of channel";
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


        internal virtual List<string> GetExtraReturnFields()
        {
            return new List<string>();
        }

        internal virtual bool GetShouldUseSearchEndDate()
        {
            CatalogCache catalogCache = CatalogCache.Instance();
            return catalogCache.IsTstvSettingsExists(m_nGroupID);
        }
    }
}