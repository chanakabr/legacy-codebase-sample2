using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;
using ApiObjects.SearchObjects;
using EpgBL;
using Logger;
using Tvinci.Core.DAL;

namespace Catalog
{
    [DataContract]
    public class EpgSearchRequest : BaseRequest, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
              
        [DataMember]
        public string m_sSearch;
        [DataMember]
        public DateTime m_dStartDate;
        [DataMember]
        public DateTime m_dEndDate;
        [DataMember]
        public int m_nProgramID;
        [DataMember]
        public List<long> m_oEPGChannelIDs;

        public EpgSearchRequest() : base()
        {
            m_nProgramID = 0;           

            m_dStartDate = DateTime.UtcNow;
            m_dEndDate = DateTime.UtcNow.AddDays(7);

            m_sSearch = string.Empty;
            m_oEPGChannelIDs = new List<long>();
        }

        public EpgSearchRequest(bool bSearchAnd, string sSearch, DateTime dStartDate, DateTime dEndDate, int nProgramID, int nPageSize, int nPageIndex, int nGroupID, string sSignature, string sSignString, List<long> epgChannelIDs)
            : base(nPageSize, nPageIndex, string.Empty, nGroupID, null, sSignature, sSignString)          
        {
            Initialize(bSearchAnd, sSearch, dStartDate, dEndDate, nProgramID, epgChannelIDs);
        }

        private void Initialize(bool bSearchAnd, string sSearch, DateTime dStartDate, DateTime dEndDate, int nProgramID,
            List<long> epgChannelIDs)
        {
           
            this.m_dEndDate = dEndDate;
            this.m_dStartDate = dStartDate;
            this.m_sSearch = sSearch;
            this.m_nProgramID = nProgramID;
            this.m_oEPGChannelIDs = epgChannelIDs;
        }

        private void Initialize(bool bSearchAnd, string sSearch, DateTime dStartDate, DateTime dEndDate, int nProgramID)
        {
            Initialize(bSearchAnd, sSearch, dStartDate, dEndDate, nProgramID, null);
        }



        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                EpgSearchRequest request = (EpgSearchRequest)oBaseRequest;
                EpgSearchResponse oResponse = new EpgSearchResponse();

                CheckSignature(oBaseRequest);
                if (request == null)
                    throw new Exception("request object is null or Required variables is null");

                    //GetMediaIds With Searcher service
                    bool isLucene = false;
                    SearchResultsObj epgSearchResponse = Catalog.GetProgramIdsFromSearcher(request, ref isLucene);
                    if (epgSearchResponse == null)
                    {
                        oResponse = new EpgSearchResponse();
                    }
                    else
                    {
                        oResponse.m_nTotalItems = epgSearchResponse.n_TotalItems;

                        //Complete max updatedate per mediaId
                        if (epgSearchResponse.m_resultIDs != null)
                        {
                            switch (isLucene)
                            {
                                case true: // if Lucene need to complete UpdateDate from DB
                                    List<SearchResult> lProgramRes = GetProgramUpdateDate(epgSearchResponse.m_resultIDs.Select(item => item.assetID).ToList());
                                    oResponse.m_nEpgIds = lProgramRes;
                                    break;
                                default:
                                    oResponse.m_nEpgIds = epgSearchResponse.m_resultIDs;
                                    break;
                            }
                        }
                    }
                
                return (BaseResponse)oResponse;
            }
            catch (Exception ex)
            {
                _logger.Error("GetSearchMediaWithSearcher", ex);
                throw ex;
            }
        }

        private List<SearchResult> GetProgramUpdateDate(List<int> lPrograms)
        {
            List<SearchResult> lProgramRes = new List<SearchResult>();
            SearchResult oProgramRes = new SearchResult();

            DataTable dt = CatalogDAL.Get_EpgProgramUpdateDate(lPrograms);
            if (dt != null)
            {
                if (dt.Columns != null)
                {  
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        oProgramRes.assetID = Utils.GetIntSafeVal(dt.Rows[i], "ID");
                        if (!string.IsNullOrEmpty(dt.Rows[i]["UPDATE_DATE"].ToString()))
                        {
                            oProgramRes.UpdateDate = System.Convert.ToDateTime(dt.Rows[i]["UPDATE_DATE"].ToString());
                        }
                        lProgramRes.Add(oProgramRes);
                        oProgramRes = new SearchResult();
                    }
                }
            }
            return lProgramRes;
        }
    }

}
