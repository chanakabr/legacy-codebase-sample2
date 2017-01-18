using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;
using EpgBL;
using KLogMonitor;
using Tvinci.Core.DAL;
using Core.Catalog.Cache;
using GroupsCacheManager;

namespace Core.Catalog.Request
{
    [DataContract]
    public class EpgSearchRequest : BaseRequest, IRequestImp, IEpgSearchable
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public bool m_bExact;

        [DataMember]
        public List<KeyValue> m_AndList;
        [DataMember]
        public List<KeyValue> m_OrList;

        [DataMember]
        public string m_sSearch;

        [DataMember]
        public DateTime m_dStartDate;
        [DataMember]
        public DateTime m_dEndDate;

        [DataMember]
        public List<long> m_oEPGChannelIDs;

        private bool m_bIsLucene;

        public EpgSearchRequest()
            : base()
        {
            m_bExact = false; // search with "like" on text

            m_AndList = null;
            m_OrList = null;

            m_dStartDate = DateTime.UtcNow;
            m_dEndDate = DateTime.UtcNow.AddDays(7);

            m_sSearch = string.Empty;
            m_oEPGChannelIDs = new List<long>();
            m_bIsLucene = false;
        }

        public EpgSearchRequest(bool bSearchAnd, string sSearch, DateTime dStartDate, DateTime dEndDate, int nPageSize, int nPageIndex, int nGroupID, string sSignature, string sSignString, List<long> epgChannelIDs,
            List<KeyValue> andList, List<KeyValue> orList)
            : base(nPageSize, nPageIndex, string.Empty, nGroupID, null, sSignature, sSignString)
        {
            Initialize(bSearchAnd, sSearch, dStartDate, dEndDate, epgChannelIDs, andList, orList);
        }

        private void Initialize(bool bSearchAnd, string sSearch, DateTime dStartDate, DateTime dEndDate, /*int nProgramID,*/
            List<long> epgChannelIDs, List<KeyValue> andList, List<KeyValue> orList)
        {

            this.m_dEndDate = dEndDate;
            this.m_dStartDate = dStartDate;
            this.m_sSearch = sSearch;
            this.m_oEPGChannelIDs = epgChannelIDs;
            this.m_AndList = andList;
            this.m_OrList = orList;
        }

        private void Initialize(bool bSearchAnd, string sSearch, DateTime dStartDate, DateTime dEndDate, List<KeyValue> andList, List<KeyValue> orList)
        {
            Initialize(bSearchAnd, sSearch, dStartDate, dEndDate, null, andList, orList);
        }

        protected void CheckEPGRequestIsValid()
        {
            if (m_nGroupID == 0 ||
                (string.IsNullOrEmpty(m_sSearch) && (m_AndList == null || m_AndList.Count == 0) &&
                (m_OrList == null || m_OrList.Count == 0)))
            {
                throw new ArgumentException("Request object is null or missing search text or group id");
            }
        }


        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                EpgSearchResponse oResponse = new EpgSearchResponse();

                CheckEPGRequestIsValid();
                CheckSignature(this);
                SearchResultsObj epgSearchResponse = null;
                epgSearchResponse = CatalogLogic.GetProgramIdsFromSearcher(BuildEPGSearchObject());
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
                        oResponse.m_nEpgIds = epgSearchResponse.m_resultIDs;
                    }
                }
                return oResponse;
            }
            catch (Exception ex)
            {
                log.Error("GetSearchMediaWithSearcher", ex);
                throw ex;
            }
        }

        private List<SearchResult> GetProgramUpdateDate(List<int> lPrograms)
        {
            List<SearchResult> lProgramRes = new List<SearchResult>();
            SearchResult oProgramRes = new SearchResult();

            DataTable dt = CatalogDAL.Get_EpgProgramUpdateDate(lPrograms);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
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
            return lProgramRes;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Concat(" Search Text: ", m_sSearch));
            sb.Append(String.Concat(" Start Date: ", m_dStartDate != null ? m_dStartDate.ToString() : "null"));
            sb.Append(String.Concat(" End Date: ", m_dEndDate != null ? m_dEndDate.ToString() : "null"));
            // sb.Append(String.Concat(" Program ID: ", m_nProgramID));
            sb.Append(" EPG Channel IDs: ");
            if (m_oEPGChannelIDs != null && m_oEPGChannelIDs.Count > 0)
            {
                for (int i = 0; i < m_oEPGChannelIDs.Count; i++)
                    sb.Append(String.Concat(m_oEPGChannelIDs[i], ";"));
            }
            else
            {
                sb.Append(m_oEPGChannelIDs != null ? "empty" : "null");
            }
            sb.Append(String.Concat(" ", base.ToString()));

            return sb.ToString();
        }

        public EpgSearchObj BuildEPGSearchObject()
        {
            EpgSearchObj res = null;
            ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();

            if (searcher == null)
            {
                throw new Exception(String.Concat("Failed to create Searcher instance. Request is: ", ToString()));
            }
            List<List<string>> jsonizedChannelsDefinitions = null;
            if (CatalogLogic.IsUseIPNOFiltering(this, ref searcher, ref jsonizedChannelsDefinitions))
            {
                m_oEPGChannelIDs = CatalogLogic.GetEpgChannelIDsForIPNOFiltering(m_nGroupID, ref searcher,
                    this.domainId, this.m_sSiteGuid,
                    ref jsonizedChannelsDefinitions);
                res = BuildEPGSearchObjectInner();

            }
            else
            {
                res = BuildEPGSearchObjectInner();
            }

            return res;
        }

        private EpgSearchObj BuildEPGSearchObjectInner()
        {
            List<string> lSearchList = new List<string>();
            EpgSearchObj searcherEpgSearch = new EpgSearchObj();

            searcherEpgSearch.m_bExact = m_bExact;
            searcherEpgSearch.m_dEndDate = m_dEndDate;
            searcherEpgSearch.m_dStartDate = m_dStartDate;

            searcherEpgSearch.m_dSearchEndDate = DateTime.UtcNow;


            //deafult values for OrderBy object 
            searcherEpgSearch.m_bDesc = true;
            searcherEpgSearch.m_sOrderBy = "start_date";

            // set parent group by request.m_nGroupID                              
            searcherEpgSearch.m_nGroupID = m_nGroupID;

            List<SearchValue> dAnd = new List<SearchValue>();
            List<SearchValue> dOr = new List<SearchValue>();
            if (m_bExact) // free text search - based on  Exact tags and metas 
            {
                EpgSearchAddParams(ref dAnd, ref dOr);
            }
            else  // free text search - based on  metas/tags "isSearchable" setting 
            {
                searcherEpgSearch.m_bSearchAnd = false; //Search by OR 
                //Get all tags and meta for group
                CatalogLogic.GetGroupsTagsAndMetas(m_nGroupID, ref lSearchList);

                if (lSearchList == null)
                {
                    throw new Exception(String.Concat("Failed to retrieve groups tags and metas from DB. Req: ", ToString()));
                }
                foreach (string item in lSearchList)
                {
                    dOr.Add(new SearchValue(item, m_sSearch));
                }
            }
            //initialize the search list with And / Or values
            searcherEpgSearch.m_lSearchOr = dOr;
            searcherEpgSearch.m_lSearchAnd = dAnd;

            searcherEpgSearch.m_nPageIndex = m_nPageIndex;
            searcherEpgSearch.m_nPageSize = m_nPageSize;

            searcherEpgSearch.m_oEpgChannelIDs = m_oEPGChannelIDs;

            searcherEpgSearch.m_nNextTop = 0;
            searcherEpgSearch.m_nPrevTop = 0;
            searcherEpgSearch.m_bIsCurrent = false;
            searcherEpgSearch.m_bSearchOnlyDatesAndChannels = false;

            // Get the linear channels of the current region(s)
            CatalogLogic.SetEpgSearchChannelsByRegions(ref searcherEpgSearch, this);

            return searcherEpgSearch;
        }

        private void EpgSearchAddParams(ref List<SearchValue> m_dAnd, ref List<SearchValue> m_dOr)
        {
            Group group = GroupsCache.Instance().GetGroup(m_nGroupID);

            if (m_AndList != null)
            {
                foreach (KeyValue andKeyValue in m_AndList)
                {
                    bool isTagOrMeta = false;
                    SearchValue search = new SearchValue();
                    search.m_sKey = andKeyValue.m_sKey;
                    search.m_lValue = new List<string> { andKeyValue.m_sValue };
                    search.m_sValue = andKeyValue.m_sValue;

                    foreach (var tag in group.m_oEpgGroupSettings.m_lTagsName)
                    {
                        if (tag.Equals(search.m_sKey, StringComparison.OrdinalIgnoreCase))
                        {
                            isTagOrMeta = true;
                            search.m_sKey = string.Format("tags.{0}", search.m_sKey);
                            break;
                        }
                    }

                    if (!isTagOrMeta)
                    {
                        foreach (var meta in group.m_oEpgGroupSettings.m_lMetasName)
                        {
                            if (meta.Equals(search.m_sKey, StringComparison.OrdinalIgnoreCase))
                            {
                                isTagOrMeta = true;
                                search.m_sKey = string.Format("metas.{0}", search.m_sKey);
                                break;
                            }
                        }
                    }

                    m_dAnd.Add(search);
                }
            }

            if (m_OrList != null)
            {
                foreach (KeyValue orKeyValue in m_OrList)
                {
                    SearchValue search = new SearchValue();
                    search.m_sKey = orKeyValue.m_sKey;
                    search.m_lValue = new List<string> { orKeyValue.m_sValue };
                    search.m_sValue = orKeyValue.m_sValue;

                    foreach (var tag in group.m_oEpgGroupSettings.m_lTagsName)
                    {
                        if (tag.Equals(search.m_sKey, StringComparison.OrdinalIgnoreCase))
                        {
                            search.m_sKey = string.Format("tags.{0}", search.m_sKey);
                            break;
                        }
                    }

                    m_dOr.Add(search);

                    foreach (var meta in group.m_oEpgGroupSettings.m_lMetasName)
                    {
                        if (meta.Equals(search.m_sKey, StringComparison.OrdinalIgnoreCase))
                        {
                            search = new SearchValue();
                            search.m_sKey = orKeyValue.m_sKey;
                            search.m_lValue = new List<string> { orKeyValue.m_sValue };
                            search.m_sValue = orKeyValue.m_sValue;

                            search.m_sKey = string.Format("metas.{0}", search.m_sKey);

                            m_dOr.Add(search);
                            break;
                        }
                    }
                }
            }
        }
    }
}
