using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;
using Catalog.Response;
using EpgBL;
using KLogMonitor;

namespace Catalog.Request
{
    [Serializable]
    [DataContract]
    public class EPGProgramsByProgramsIdentefierRequest : BaseEpg, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        string[] pids { get; set; }
       
        public EPGProgramsByProgramsIdentefierRequest()
            : base()
        {
        }

        public EPGProgramsByProgramsIdentefierRequest(EPGProgramsByProgramsIdentefierRequest epg)
            : base(epg.eLang, epg.duration, epg.m_nPageSize, epg.m_nPageIndex, epg.m_sUserIP, epg.m_nGroupID, epg.m_oFilter, epg.m_sSignature, epg.m_sSignString)
        {
            this.pids = epg.pids;
        }

         public BaseResponse GetResponse(BaseRequest oBaseRequest)
         {
             try
             {
                 EPGProgramsByProgramsIdentefierRequest request = oBaseRequest as EPGProgramsByProgramsIdentefierRequest;

                 if (request == null)
                     throw new ArgumentException("request object is null or Required variables is null");

                 CheckSignature(request);

                 EpgProgramsResponse response = new EpgProgramsResponse();
                 BaseEpgBL epgBL = EpgBL.Utils.GetInstance(request.m_nGroupID);

                 List<EPGChannelProgrammeObject> retList = epgBL.GetEPGPrograms(request.m_nGroupID,  request.pids, request.eLang, request.duration);
                 if (retList != null && retList.Count > 0)
                 {
                     // get all linear settings about channel + group
                     Catalog.GetLinearChannelSettings(request.m_nGroupID, retList);

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
