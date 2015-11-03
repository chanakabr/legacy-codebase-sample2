using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Catalog.Response;
using KLogMonitor;
using ApiObjects.Response;

namespace Catalog.Request
{
    [DataContract]
    public class DomainLastPositionRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public MediaLastPositionRequestData data { get; set; }
        [DataMember]
        public int m_nDomainID { get; set; }

        public DomainLastPositionRequest()
            : base()
        {

        }

        public DomainLastPositionRequest(MediaLastPositionRequest m)
            : base(m.m_nPageSize, m.m_nPageIndex, m.m_sUserIP, m.m_nGroupID, m.m_oFilter, m.m_sSignature, m.m_sSignString)
        {
            data = m.data;
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                DomainLastPositionRequest req = null;
                DomainLastPositionResponse res = new DomainLastPositionResponse();

                CheckSignature(oBaseRequest);

                if (oBaseRequest != null)
                {
                    req = (DomainLastPositionRequest)oBaseRequest;
                    res = ProcessDomainLastPositionRequest(req);
                }
                else
                {
                    res.m_sStatus = "BAD_REQUEST";
                    res.m_sDescription = "Null request";
                }

                return (BaseResponse)res;
            }
            catch (Exception ex)
            {
                log.Error("DomainLastPositionRequest.GetResponse", ex);
                throw ex;
            }
        }

        private DomainLastPositionResponse ProcessDomainLastPositionRequest(DomainLastPositionRequest request)
        {
            DomainLastPositionResponse response = new DomainLastPositionResponse();
            try
            {
                int nSiteGuid = 0;

                if (request.data == null || request.data.m_nMediaID == 0 || request.m_nDomainID == 0)
                {
                    response.m_sStatus = "INVALID_PARAMS";
                    return response;
                }
                //non-anonymous user
                if (!Catalog.IsAnonymousUser(request.data.m_sSiteGuid, out nSiteGuid))
                {
                    response = Catalog.GetLastDomainPosition(nSiteGuid, request.data.m_nMediaID, request.m_nDomainID, request.m_nGroupID, request.data.m_sUDID);
                }
                else
                {
                    response.Status = new Status((int)eResponseStatus.InvalidUser, "Invalid user");
                }

                return response;
            }
            catch (Exception ex)
            {
                log.Error("ProcessDomainLastPositionRequest - " + String.Concat("Failed ex={0}, siteGuid={1}, m_nMediaID={2}, m_nDomainID={3} ", ex.Message,
                    request.data != null ? request.data.m_sSiteGuid : "0", request.data != null ? request.data.m_nMediaID : 0, request.m_nDomainID), ex);
                response.m_sStatus = "Error";
                response.m_sDescription = ex.Message;
                return response;
            }
        }
    }
}
