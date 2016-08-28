using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using System.Data;
using TVinciShared;
using DAL;
using Tvinci.Core.DAL;
using Catalog.Response;
using KLogMonitor;

namespace Catalog.Request
{
    [DataContract]
    public class MediaLastPositionRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public MediaLastPositionRequestData data
        {
            get;
            set;
        }

        public MediaLastPositionRequest()
            : base()
        {

        }

        public MediaLastPositionRequest(MediaLastPositionRequest m)
            : base(m.m_nPageSize, m.m_nPageIndex, m.m_sUserIP, m.m_nGroupID, m.m_oFilter, m.m_sSignature, m.m_sSignString)
        {
            data = m.data;
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                MediaLastPositionRequest req = null;
                MediaLastPositionResponse res = null;

                CheckSignature(oBaseRequest);

                if (oBaseRequest != null)
                {
                    req = (MediaLastPositionRequest)oBaseRequest;
                    if (req == null || req.data == null)
                    {
                        res = new MediaLastPositionResponse();
                        res.m_sStatus = "BAD_REQUEST";
                        res.m_sDescription = "Null request";
                        return res;
                    }
                    bool bNpvr = string.IsNullOrEmpty(req.data.m_sNpvrID) ? false : true;
                    if (!bNpvr)// Media
                    {
                        res = ProcessMediaLastPositionRequest(req);
                    }
                    else // Npvr
                    {
                        res = ProcessNpvrLastPositionRequest(req);
                    }
                }
                else
                {
                    res = new MediaLastPositionResponse();
                    res.m_sStatus = "BAD_REQUEST";
                    res.m_sDescription = "Null request";
                }

                return (BaseResponse)res;
            }
            catch (Exception ex)
            {
                log.Error("MediaLastPositionRequest.GetResponse", ex);
                throw ex;
            }
        }

        private MediaLastPositionResponse ProcessNpvrLastPositionRequest(MediaLastPositionRequest request)
        {
            MediaLastPositionResponse response = new MediaLastPositionResponse();
            int nSiteGuid = 0;
            int pos = 0;

            if (request.data == null || string.IsNullOrEmpty(request.data.m_sNpvrID))
            {
                response.m_sStatus = "INVALID_PARAMS";
            }
            //non-anonymous user
            else if (!Catalog.IsAnonymousUser(request.data.m_sSiteGuid, out nSiteGuid))
            {
                pos = Catalog.GetLastPosition(request.data.m_sNpvrID, nSiteGuid);
            }

            response.Location = pos;

            return response;
        }

        private MediaLastPositionResponse ProcessMediaLastPositionRequest(MediaLastPositionRequest request)
        {
            MediaLastPositionResponse response = new MediaLastPositionResponse();
            int nSiteGuid = 0;
            int pos = 0;

            if (request.data.m_nMediaID == 0)
            {
                response.m_sStatus = "INVALID_PARAMS";
            }
            //non-anonymous user
            else if (!string.IsNullOrEmpty(request.data.m_sSiteGuid) && Int32.TryParse(request.data.m_sSiteGuid, out nSiteGuid) || nSiteGuid != 0)
            {
                pos = Catalog.GetLastPosition(request.data.m_nMediaID, nSiteGuid);
            }

            response.Location = pos;

            return response;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(data != null ? data.ToString() : " data is null");

            return sb.ToString();
        }
    }
}
