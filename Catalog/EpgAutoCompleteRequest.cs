using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Logger;

namespace Catalog
{
    [DataContract]
    public class EpgAutoCompleteRequest :  BaseRequest, IRequestImp
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

         public EpgAutoCompleteRequest() : base()
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

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                EpgAutoCompleteRequest request = oBaseRequest as EpgAutoCompleteRequest;
                EpgAutoCompleteResponse oResponse = new EpgAutoCompleteResponse();

                if (request == null || string.IsNullOrEmpty(request.m_sSearch) || request.m_nGroupID == 0)
                {
                    throw new Exception("request object null or miss 'must' parameters ");
                }
                CheckSignature(request);

                //Auto Complete with Searcher               
                List<string> epgAutoList = Catalog.EpgAutoComplete(request);

                if (epgAutoList != null)
                {   
                    oResponse.m_sList.AddRange(epgAutoList);
                }
                return (BaseResponse)oResponse;
            }
            catch (Exception ex)
            {
                _logger.Error("GetSearchMediaWithSearcher", ex);
                throw ex;
            }
        }

    }
}
