using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    [DataContract]
    public class EpgAutoCompleteRequest : BaseRequest, IRequestImp, IEpgSearchable
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
 

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

        public EpgAutoCompleteRequest()
            : base()
        {
            m_nProgramID = 0;

            m_dStartDate = DateTime.UtcNow;
            m_dEndDate = DateTime.UtcNow.AddDays(7);
            m_sSearch = string.Empty;

        }

        public EpgAutoCompleteRequest(bool bSearchAnd, string sSearch, DateTime dStartDate, DateTime dEndDate, int nProgramID, int nPageSize, int nPageIndex, int nGroupID, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, string.Empty, nGroupID, null, sSignature, sSignString)
        {
            Initialize(bSearchAnd, sSearch, dStartDate, dEndDate, nProgramID);
        }

        private void Initialize(bool bSearchAnd, string sSearch, DateTime dStartDate, DateTime dEndDate, int nProgramID)
        {
            this.m_dEndDate = dEndDate;
            this.m_dStartDate = dStartDate;
            this.m_sSearch = sSearch;
            this.m_nProgramID = nProgramID;
        }

        private void CheckRequestValidness()
        {
            if (string.IsNullOrEmpty(m_sSearch) || m_nGroupID == 0)
            {
                throw new ArgumentException("request object null or miss 'must' parameters ");
            }
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                EpgAutoCompleteResponse oResponse = new EpgAutoCompleteResponse();

                CheckRequestValidness();
                CheckSignature(this);

                //Auto Complete with Searcher               
                List<string> epgAutoList = CatalogLogic.EpgAutoComplete(BuildEPGSearchObject());

                if (epgAutoList != null)
                {
                    oResponse.m_sList.AddRange(epgAutoList);
                }

                return oResponse;
            }
            catch (Exception ex)
            {
                log.Error("GetSearchMediaWithSearcher", ex);
                throw ex;
            }
        }


        public EpgSearchObj BuildEPGSearchObject()
        {
            EpgSearchObj res = null;
            ISearcher searcher = Bootstrapper.GetInstance<ISearcher>();
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
            EpgSearchObj oEpgSearch = new EpgSearchObj();


            oEpgSearch.m_bSearchAnd = false; //search with or
            oEpgSearch.m_bDesc = false;
            oEpgSearch.m_sOrderBy = "name";

            oEpgSearch.m_dEndDate = m_dEndDate;
            oEpgSearch.m_dStartDate = m_dStartDate;
            List<EpgSearchValue> dEsv = new List<EpgSearchValue>();
            //Get all tags and meta for group
            CatalogLogic.GetGroupsTagsAndMetas(m_nGroupID, ref lSearchList);

            if (lSearchList == null)
                throw new Exception(String.Concat("Failed to retrieve groups tags and metas from DB. Req: ", ToString()));
            foreach (string item in lSearchList)
            {
                dEsv.Add(new EpgSearchValue(item, m_sSearch));
            }

            oEpgSearch.m_lSearch = dEsv;

            // set parent group by request.m_nGroupID                
            oEpgSearch.m_nGroupID = m_nGroupID;

            oEpgSearch.m_nPageIndex = m_nPageIndex;
            oEpgSearch.m_nPageSize = m_nPageSize;
            oEpgSearch.m_oEpgChannelIDs = m_oEPGChannelIDs;

            return oEpgSearch;
        }
    }
}
