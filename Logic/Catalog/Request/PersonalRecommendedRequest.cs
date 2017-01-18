using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data;
using Tvinci.Core.DAL;
using ApiObjects.SearchObjects;
using Core.Catalog.Cache;
using GroupsCacheManager;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    /*********************************************************************
     * Get Personal Recommended Request 
     * Get SiteGuid and return all the :
     * The last Media that this siteGuid watched OR
     * If there isn't any media return the top most viewd media that 
     * have been watched by siteGuids in 
     * this group of groupID
     * *******************************************************************/
    [DataContract]
    public class PersonalRecommendedRequest : BaseProtocolRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public PersonalRecommendedRequest()
            : base()
        {
        }

        public PersonalRecommendedRequest(PersonalRecommendedRequest p)
            : base(p.m_nPageSize, p.m_nPageIndex, p.m_sUserIP, p.m_nGroupID, p.m_oFilter, p.m_sSignature, p.m_sSignString, p.m_sSiteGuid, p.domainId)
        {
        }

        private MediaRelatedRequest GetMediaRelatedReqObj(PersonalRecommendedRequest request, int nMediaID)
        {
            MediaRelatedRequest oMediaRelatedRequest = new MediaRelatedRequest();
            oMediaRelatedRequest.m_nGroupID = request.m_nGroupID;
            oMediaRelatedRequest.m_nMediaID = nMediaID;
            oMediaRelatedRequest.m_nPageIndex = request.m_nPageIndex;
            oMediaRelatedRequest.m_nPageSize = request.m_nPageSize;
            oMediaRelatedRequest.m_sSignString = request.m_sSignString;
            oMediaRelatedRequest.m_sSignature = request.m_sSignature;
            oMediaRelatedRequest.m_sUserIP = request.m_sUserIP;
            oMediaRelatedRequest.m_oFilter = request.m_oFilter;
            oMediaRelatedRequest.m_sSiteGuid = request.m_sSiteGuid;
            oMediaRelatedRequest.domainId = request.domainId;

            return oMediaRelatedRequest;
        }

        private List<SearchResult> HandleGetRelated(PersonalRecommendedRequest request, int nMediaID)
        {
            MediaRelatedRequest oMediaRelatedRequest = GetMediaRelatedReqObj(request, nMediaID);
            UnifiedSearchResponse relatedResponse = (UnifiedSearchResponse)oMediaRelatedRequest.GetResponse((BaseRequest)oMediaRelatedRequest);
            if (relatedResponse == null)
                throw new Exception("MediaRelatedRequest's response is null");

            List<SearchResult> searchResults = relatedResponse.searchResults.Select(result =>
                new SearchResult()
                {
                    assetID = int.Parse(result.AssetId),
                    UpdateDate = result.m_dUpdateDate
                }).ToList();

            return searchResults;
        }

        private List<SearchResult> HandleMostViewed(DataTable dt)
        {
            int length = dt.Rows.Count;
            List<SearchResult> lMedias = new List<SearchResult>(length);
            for (int i = 0; i < length; i++)
            {
                SearchResult oMediaRes = new SearchResult();
                oMediaRes.assetID = Utils.GetIntSafeVal(dt.Rows[i], "media_id");
                string sUpdateDate = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["Update_Date"]);
                if (sUpdateDate.Length > 0)
                {
                    oMediaRes.UpdateDate = System.Convert.ToDateTime(sUpdateDate);
                }

                lMedias.Add(oMediaRes);
            }

            return lMedias;
        }


        protected override List<SearchResult> ExecuteIPNOProtocol(BaseRequest oBaseRequest, int nOperatorID, List<List<string>> jsonizedChannelsDefinitions, ref ISearcher initializedSearcher)
        {
            List<SearchResult> lMedias = null;
            try
            {
                PersonalRecommendedRequest request = oBaseRequest as PersonalRecommendedRequest;
                if (request == null)
                    throw new Exception("Request object is null");


                GroupManager groupManager = new GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                List<int> lSubGroup = groupManager.GetSubGroup(nParentGroupID);
                DataTable dt = CatalogDAL.Get_IPersonalRecommended(request.m_nGroupID, request.m_sSiteGuid, request.m_nPageSize * request.m_nPageIndex + request.m_nPageSize, nOperatorID, lSubGroup);

                if (dt != null && dt.Columns != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["getRelated"]) == 1)
                    {
                        lMedias = HandleGetRelated(request, ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["media_id"]));
                    }
                    else
                    {
                        //Return most viewed items and validate against ES they are still associated with the operator
                        lMedias = HandleMostViewed(dt);
                        lMedias = GetProtocolFinalResultsUsingSearcher(lMedias, ref initializedSearcher, jsonizedChannelsDefinitions, request.m_nGroupID);
                    }
                }
                else
                {
                    lMedias = new List<SearchResult>(0);
                }
            }
            catch (Exception ex)
            {
                log.Error(this.ToString(), ex);
                throw ex;
            }

            return lMedias;
        }

        protected override List<SearchResult> ExecuteNonIPNOProtocol(BaseRequest oBaseRequest)
        {
            List<SearchResult> lMedias = null;
            try
            {
                PersonalRecommendedRequest request = (PersonalRecommendedRequest)oBaseRequest;
                if (request == null)
                    throw new Exception("Request object is null");

                log.Info(String.Concat(request.ToString(), "started at: ", DateTime.UtcNow));

                GroupManager groupManager = new GroupManager();
                CatalogCache catalogCache = CatalogCache.Instance();
                int nParentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                List<int> lSubGroup = groupManager.GetSubGroup(nParentGroupID);

                DataTable dt = CatalogDAL.Get_PersonalRecommended(request.m_nGroupID, request.m_sSiteGuid, request.m_nPageSize * request.m_nPageIndex + request.m_nPageSize, lSubGroup);

                if (dt != null && dt.Columns != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    if (ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["getRelated"]) == 1)
                    {
                        lMedias = HandleGetRelated(request, ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["media_id"]));
                    }
                    else
                    {
                        //Return most viewed items
                        lMedias = HandleMostViewed(dt);
                    }
                }
                else
                {
                    lMedias = new List<SearchResult>(0);
                }
            }
            catch (Exception ex)
            {
                log.Error(this.ToString(), ex);
                throw ex;
            }

            return lMedias;
        }

  

        protected override int GetProtocolMaxResultsSize()
        {
            int res = 0;
            string resultsSize = Utils.GetWSURL("PERSONAL_RECOMMENDED_MAX_RESULTS_SIZE");
            if (resultsSize.Length > 0 && Int32.TryParse(resultsSize, out res))
                return res;
            return CatalogLogic.DEFAULT_PERSONAL_RECOMMENDED_MAX_RESULTS_SIZE;
        }
    }
}
