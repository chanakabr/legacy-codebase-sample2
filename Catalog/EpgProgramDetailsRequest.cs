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
    public class EpgProgramDetailsRequest : BaseRequest, IProgramsRequest
    {
         private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DataMember]
        public List<Int32> m_lProgramsIds;       


        public EpgProgramDetailsRequest(Int32 nPageSize, Int32 nPageIndex,string sUserIP, Int32 nGroupID, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, null, sSignature, sSignString)
        {
        }
        public EpgProgramDetailsRequest() 
            : base()
        {  
        }

        /*Get Program Details By ProgramsIds*/
        public EpgProgramResponse GetProgramsByIDs(EpgProgramDetailsRequest programRequest)
        {
            EpgProgramResponse pResponse = new EpgProgramResponse();           
          
            _logger.InfoFormat("{0}: {1}", "Catalog.GetProgramsByIDs Start At", DateTime.Now);
            try
            {
                if (programRequest == null || programRequest.m_lProgramsIds == null || programRequest.m_lProgramsIds.Count == 0)
                    throw new Exception("request object is null or Required variables is null");

                _logger.Info(string.Format("{0}: {1}", "count of MediasIDs", programRequest.m_lProgramsIds.Count));

                string sCheckSignature = Utils.GetSignature(programRequest.m_sSignString, programRequest.m_nGroupID);
                if (sCheckSignature != programRequest.m_sSignature)             
                    throw new Exception("Signatures dosen't match");

                _logger.InfoFormat("Start Complete Details for {0} MediaIds", programRequest.m_lProgramsIds.Count);

                bool completeDetails = Catalog.CompleteDetailsForProgramResponse(programRequest, ref pResponse);

                return pResponse;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
