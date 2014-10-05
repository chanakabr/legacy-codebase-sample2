using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using Logger;
using ApiObjects;
using ApiObjects.SearchObjects;

namespace Catalog
{
    [DataContract]
    public class EpgRequest : BaseRequest, IRequestImp
    {

        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

        private void CheckRequestValidness(EpgRequest request)
        {
            if (request == null || request.m_nGroupID < 1 || request.m_nChannelIDs == null || request.m_nChannelIDs.Count == 0)
                throw new ArgumentException("Request is null or does not contain any channels or has invalid group id");
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            EpgRequest request = oBaseRequest as EpgRequest;
            EpgResponse response = new EpgResponse();
            List<EpgResultsObj> result = null;
            try
            {
                CheckSignature(request);

                result = Catalog.GetEPGPrograms(request);
                if (result != null)
                {
                    response.programsPerChannel = result;
                    response.m_nTotalItems = result.Count;
                }
                else
                {
                    response = null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Exception thrown at EpgRequest.GetResponse", ex);
                response = null;
            }

            return response;
        }

    }



}
