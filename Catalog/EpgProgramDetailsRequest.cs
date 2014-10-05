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

        private void CheckRequestValidness(EpgProgramDetailsRequest programRequest)
        {
            if (programRequest == null || programRequest.m_lProgramsIds == null || programRequest.m_lProgramsIds.Count == 0)
                throw new ArgumentException("request object is null or required variables are missing");
        }

        /*Get Program Details By ProgramsIds*/
        public EpgProgramResponse GetProgramsByIDs(EpgProgramDetailsRequest programRequest)
        {
            EpgProgramResponse pResponse = new EpgProgramResponse();           
          
            try
            {
                CheckRequestValidness(programRequest);

                CheckSignature(programRequest);

                Catalog.CompleteDetailsForProgramResponse(programRequest, ref pResponse);

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
