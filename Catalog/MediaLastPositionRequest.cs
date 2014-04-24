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
using Logger;
using TVinciShared;
using DAL;
using Tvinci.Core.DAL;

namespace Catalog
{
    [DataContract]
    public class MediaLastPositionRequest : BaseRequest, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [DataMember]
        public MediaLastPositionRequestData data { get; set; }

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
                    res = ProcessMediaLastPositionRequest(req);
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
                _logger.Error("MediaLastPositionRequest.GetResponse", ex);
                throw ex;
            }
        }

        private MediaLastPositionResponse ProcessMediaLastPositionRequest(MediaLastPositionRequest request)
        {
            MediaLastPositionResponse response = new MediaLastPositionResponse();
            int nSiteGuid = 0;

            if (request.data.m_nMediaID == 0 || string.IsNullOrEmpty(request.data.m_sSiteGuid) || !Int32.TryParse(request.data.m_sSiteGuid, out nSiteGuid) || nSiteGuid == 0)
            {
                response.m_sStatus = "INVALID_PARAMS";
                return response;
            }

            var pos = Catalog.GetLastPosition(request.data.m_nMediaID, nSiteGuid);
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
