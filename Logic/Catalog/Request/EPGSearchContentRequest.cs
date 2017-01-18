using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;
using Core.Catalog.Response;
using EpgBL;
using KLogMonitor;

namespace Core.Catalog.Request
{
    [Serializable]
    [DataContract]
    public class EPGSearchContentRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public string m_sSearch;       
        [DataMember]
        public List<long> m_oEPGChannelIDs;

        public EPGSearchContentRequest()
            : base()
        {  
            m_sSearch = string.Empty;
            m_oEPGChannelIDs = new List<long>();
        }

        public EPGSearchContentRequest(bool bSearchAnd, string sSearch, int nPageSize, int nPageIndex, int nGroupID, string sSignature, string sSignString, List<long> epgChannelIDs)
            : base(nPageSize, nPageIndex, string.Empty, nGroupID, null, sSignature, sSignString)          
        {
            Initialize(bSearchAnd, sSearch, epgChannelIDs);
        }

        private void Initialize(bool bSearchAnd, string sSearch,  List<long> epgChannelIDs)
        {
            this.m_sSearch = sSearch;
            this.m_oEPGChannelIDs = epgChannelIDs;
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                EPGSearchContentRequest request = oBaseRequest as EPGSearchContentRequest;
                EpgProgramsResponse oResponse = new EpgProgramsResponse();

                if (request == null)
                    throw new ArgumentException("request object is null or Required variables is null");
                CheckSignature(oBaseRequest);

                //GetEpgPrograms for YES
                EpgProgramsResponse epgSearchResponse = new EpgProgramsResponse();
                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(request.m_nGroupID);   //YesBL           

                List<EPGChannelProgrammeObject> retList = epgBL.SearchEPGContent(request.m_nGroupID, request.m_sSearch, request.m_nPageIndex, request.m_nPageSize);
                if (retList != null && retList.Count > 0)
                {
                    oResponse.lEpgList = retList;
                    oResponse.m_nTotalItems = retList.Count;
                }

                return oResponse;
            }
            catch (Exception ex)
            {
                log.Error("GetSearchMediaWithSearcher", ex);
                throw ex;
            }
        }

    }
}
