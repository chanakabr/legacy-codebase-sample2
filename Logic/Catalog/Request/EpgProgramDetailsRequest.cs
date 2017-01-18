using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    [DataContract]
    public class EpgProgramDetailsRequest : BaseRequest, IProgramsRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public List<Int32> m_lProgramsIds;


        public EpgProgramDetailsRequest(Int32 nPageSize, Int32 nPageIndex, string sUserIP, Int32 nGroupID, string sSignature, string sSignString)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, null, sSignature, sSignString)
        {
        }
        public EpgProgramDetailsRequest()
            : base()
        {
        }

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

                CatalogLogic.CompleteDetailsForProgramResponse(this, ref pResponse);

                return pResponse;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at GetProgramsByIDs. ");
                sb.Append(String.Concat(" Req: ", ToString()));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
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


        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
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
                log.Error("EpgProgramDetailsRequest - " + String.Format("Failed ex={0}, siteGuid={1}, group_id={3} , ST: {4}", ex.Message,
                  oBaseRequest.m_sSiteGuid, oBaseRequest.m_nGroupID, ex.StackTrace), ex);
                return new EpgProgramResponse();
            }

        }
    }
}
