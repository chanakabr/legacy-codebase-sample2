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

        
        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            EpgRequest request = oBaseRequest as EpgRequest;
            EpgResponse response = new EpgResponse();           
            List<EpgResultsObj> result;
            using (Logger.BaseLog log = new Logger.BaseLog(eLogType.CodeLog, DateTime.UtcNow, true))
            {
                log.Method = "EpgProgramIDsRequest GetResponse";
                try
                {
                    if (request == null)
                        throw new ArgumentException("request object is null");

                    if (request.m_nChannelIDs == null || request.m_nChannelIDs.Count == 0)
                        throw new ArgumentException("Request does not contain any channels");

                    CheckSignature(request);

                    result = Catalog.GetEPGPrograms(request);
                    if (result != null)
                    {
                        response.programsPerChannel = result;
                        response.m_nTotalItems = result.Count;
                    }
                    else
                    {
                        log.Message = string.Format("Result from Catalog.GetEPGProgramIds was null. request startDate : {0}, requst endDate: {1}"
                                        , request.m_dStartDate.ToString(), request.m_dEndDate.ToString());
                        log.Error(log.Message, false);
                        response = null;
                    }
                }
                catch (Exception ex)
                {   
                    log.Message = string.Format("Could not retrieve the EPGIDs from Catalog.GetEPGProgramIds. Exception message: {0}, stack: {1}", ex.Message, ex.StackTrace);
                    log.Error(log.Message, false);
                    response = null;
                }
            }

            return response;
        }
       
    }



}
