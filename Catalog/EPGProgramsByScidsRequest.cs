using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Logger;
using ApiObjects;
using EpgBL;

namespace Catalog
{
    [Serializable]
    [DataContract]
    public class EPGProgramsByScidsRequest : BaseEpg, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        [DataMember]
        string[] scids { get; set; }
       

        public EPGProgramsByScidsRequest()
            : base()
        {
        }

        public EPGProgramsByScidsRequest(EPGProgramsByScidsRequest epg)
            : base(epg.eLang, epg.duration, epg.m_nPageSize, epg.m_nPageIndex, epg.m_sUserIP, epg.m_nGroupID, epg.m_oFilter, epg.m_sSignature, epg.m_sSignString)
        {
            this.scids = epg.scids;
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                EPGProgramsByScidsRequest request = (EPGProgramsByScidsRequest)oBaseRequest;

                if (request == null)
                    throw new Exception("request object is null or Required variables is null");

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures dosen't match");

                EpgProgramsResponse response = new EpgProgramsResponse();
                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(request.m_nGroupID);              

                List<EPGChannelProgrammeObject> retList = epgBL.GetEPGProgramsByScids(request.m_nGroupID, request.scids, request.eLang, request.duration);
                if (retList != null && retList.Count > 0)
                {
                    response.lEpgList = retList;
                    response.m_nTotalItems = retList.Count;
                }
                return (BaseResponse)response;
            }
            catch (Exception ex)
            {
                return new BaseResponse();
            }
        }
    }
}
