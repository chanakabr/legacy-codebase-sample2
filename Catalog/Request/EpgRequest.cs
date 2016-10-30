using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using ApiObjects;
using ApiObjects.SearchObjects;
using System.Diagnostics;
using EpgBL;
using Catalog.Response;
using KLogMonitor;
using Catalog.Cache;

namespace Catalog.Request
{
    [DataContract]
    public class EpgRequest : BaseRequest, IRequestImp, IEpgSearchable
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected static readonly int CURRENT_REQUEST_DAYS_OFFSET = Catalog.GetCurrentRequestDaysOffset();

        [DataMember]
        public List<int> m_nChannelIDs;

        [DataMember]
        public DateTime m_dStartDate;

        [DataMember]
        public DateTime m_dEndDate;

        [DataMember]// to distinguish the case of getting Epg by Dates or Current Epgs
        public EpgSearchType m_eSearchType;

        [DataMember]
        public int m_nNextTop; //in the case of "current"

        [DataMember]
        public int m_nPrevTop; //in the case of "current"       

        public EpgRequest()
            : base()
        {

        }

        public EpgRequest(List<int> nChannelID, DateTime dStartDate, DateTime dEndDate, EpgSearchType eSearchType, int nNextTop, int nPrevTop, int nGroupID, int nPageSize, int nPageIndex, string sUserIP, Filter oFilter, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            m_nChannelIDs = nChannelID;
            m_dStartDate = dStartDate;
            m_dEndDate = dEndDate;
            m_eSearchType = eSearchType;
            m_nNextTop = nNextTop;
            m_nPrevTop = nPrevTop;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Concat("EpgRequest. St. Date: ", m_dStartDate));
            sb.Append(String.Concat(" End Date: ", m_dEndDate));
            sb.Append(String.Concat(" Search Type: ", m_eSearchType.ToString()));
            sb.Append(String.Concat(" Next Top: ", m_nNextTop));
            sb.Append(String.Concat(" Prev Top: ", m_nPrevTop));
            if (m_nChannelIDs != null && m_nChannelIDs.Count > 0)
            {
                sb.Append(" Channels: ");
                for (int i = 0; i < m_nChannelIDs.Count; i++)
                {
                    sb.Append(m_nChannelIDs[i].ToString());
                }
            }
            else
            {
                sb.Append(" Channel list is empty. ");
            }

            sb.Append(String.Concat(" Base Req: ", base.ToString()));

            return sb.ToString();

        }

        protected override void CheckRequestValidness()
        {
            if (m_nGroupID < 1 || m_nChannelIDs == null || m_nChannelIDs.Count == 0 || (m_eSearchType == EpgSearchType.Current && (m_nNextTop == 0 || m_nPrevTop == 0)))
                throw new ArgumentException("Request either does not contain any channels or has invalid group id");
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            EpgResponse response = new EpgResponse();
            try
            {
                CheckRequestValidness();
                CheckSignature(this);
                SearchResultsObj sro = null;
                // Build EPG Search Object in one time
                sro = Catalog.GetProgramIdsFromSearcher(BuildEPGSearchObject());

                if (sro != null && sro.m_resultIDs != null && sro.m_resultIDs.Count > 0)
                {
                    response = Catalog.GetEPGProgramsFromCB(sro.m_resultIDs.Select(item => item.assetID).ToList<int>(), m_nGroupID, m_eSearchType == EpgSearchType.Current, m_nChannelIDs, oBaseRequest.m_oFilter.m_nLanguage);
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception thrown at EpgRequest.GetResponse", ex);
                throw ex;
            }

            return response;
        }
        public EpgSearchObj BuildEPGSearchObject()
        {
            EpgSearchObj res = new EpgSearchObj();
            res.m_bSearchOnlyDatesAndChannels = true;
            res.m_nGroupID = m_nGroupID;
            res.m_nPageSize = m_nPageSize;
            res.m_nPageIndex = m_nPageIndex;
            res.m_lSearchOr = new List<SearchValue>(0);
            res.m_lSearchAnd = new List<SearchValue>(0);
            res.m_oEpgChannelIDs = m_nChannelIDs.Distinct().Select(item => (long)item).ToList<long>();

            switch (m_eSearchType)
            {
                case EpgSearchType.Current:
                    {
                        res.m_bIsCurrent = true;
                        res.m_nNextTop = m_nNextTop;
                        res.m_nPrevTop = m_nPrevTop;
                        DateTime now = DateTime.UtcNow;
                        res.m_dEndDate = now.AddDays(CURRENT_REQUEST_DAYS_OFFSET);
                        res.m_dStartDate = now.AddDays(-CURRENT_REQUEST_DAYS_OFFSET);
                        break;
                    }
                default:
                    {
                        // ByDate
                        res.m_bIsCurrent = false;
                        res.m_dEndDate = m_dEndDate;
                        res.m_dStartDate = m_dStartDate;
                        break;
                    }
            }
            // for Buffer ....
            res.m_dSearchEndDate = DateTime.UtcNow;

            return res;
        }
    }



}
