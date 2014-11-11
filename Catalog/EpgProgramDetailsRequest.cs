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

        //private void CheckRequestValidness(EpgProgramDetailsRequest programRequest)
        //{
        //    if (programRequest == null || programRequest.m_lProgramsIds == null || programRequest.m_lProgramsIds.Count == 0)
        //        throw new ArgumentException("request object is null or required variables are missing");
        //}

        protected override void CheckRequestValidness()
        {
            if (m_lProgramsIds == null || m_lProgramsIds.Count == 0)
                throw new ArgumentException("No programs ids in request.");
        }

        /*Get Program Details By ProgramsIds*/
        public EpgProgramResponse GetProgramsByIDs(EpgProgramDetailsRequest programRequest)
        {
            EpgProgramResponse pResponse = new EpgProgramResponse();           
          
            try
            {
                CheckRequestValidness();

                CheckSignature(this);

                Catalog.CompleteDetailsForProgramResponse(this, ref pResponse);

                return pResponse;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at GetProgramsByIDs. ");
                sb.Append(String.Concat(" Req: ", ToString()));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                Logger.Logger.Log("Exception", sb.ToString(), "EpgProgramDetailsRequest");
                throw ex;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Concat(base.ToString(), " || "));
            
            if (m_lProgramsIds != null && m_lProgramsIds.Count > 0)
            {
                sb.Append("P IDs: ");
                for (int i = 0; i < m_lProgramsIds.Count; i++)
                {
                    sb.Append(String.Concat(m_lProgramsIds[i], ";"));
                }
            }
            else
            {
                sb.Append("No program ids.");
            }

            return sb.ToString();

        }
    }
}
