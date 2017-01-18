using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;
using EpgBL;
using Core.Catalog.Response;
using KLogMonitor;

namespace Core.Catalog.Request
{
    [Serializable]
    [DataContract]
    public class EPGProgramsByScidsRequest : BaseEpg, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
         
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

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                EPGProgramsByScidsRequest request = oBaseRequest as EPGProgramsByScidsRequest;

                if (request == null)
                    throw new Exception("request object is null or Required variables is null");

                CheckSignature(request);

                EpgProgramsResponse response = new EpgProgramsResponse();
                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(request.m_nGroupID);              

                List<EPGChannelProgrammeObject> retList = epgBL.GetEPGProgramsByScids(request.m_nGroupID, request.scids, request.eLang, request.duration);
                if (retList != null && retList.Count > 0)
                {
                    response.lEpgList = retList;
                    response.m_nTotalItems = retList.Count;
                }
                return response;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return new BaseResponse();
            }
        }
    }
}
