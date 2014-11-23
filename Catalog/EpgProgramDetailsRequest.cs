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
    public class EpgProgramDetailsRequest : BaseRequest, IProgramsRequest , IRequestImp
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

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {   
            try
            {
                EpgProgramDetailsRequest request = oBaseRequest as EpgProgramDetailsRequest;

                if (request == null)
                    throw new ArgumentException("request object is null or Required variables is null");

                EpgProgramResponse oEpgProgramResponse = GetProgramsByIDs(request);

                return oEpgProgramResponse;                
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("EpgProgramDetailsRequest", String.Concat("Failed ex={0}, siteGuid={1}, group_id={3} ", ex.Message,
                  oBaseRequest.m_sSiteGuid, oBaseRequest.m_nGroupID), "EpgProgramDetailsRequest");
                return new EpgProgramResponse();
            }
        }
    }
}
