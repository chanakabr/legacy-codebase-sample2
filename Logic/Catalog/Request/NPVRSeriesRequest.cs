using ApiObjects;
using NPVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Core.Catalog.Response;
using KLogMonitor;
using System.Reflection;

namespace Core.Catalog.Request
{
    [DataContract]
    public class NPVRSeriesRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public RecordedEPGOrderObj m_oOrderObj;

        protected override void CheckRequestValidness()
        {
            if (!NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(m_nGroupID))
            {
                throw new ArgumentException(String.Concat("Group: ", m_nGroupID, " does not have NPVR implementation."));
            }
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            NPVRSeriesResponse res = new NPVRSeriesResponse();

            try
            {
                CheckRequestValidness();
                CheckSignature(this);
                
                res = CatalogLogic.GetSeriesRecordings(m_nGroupID, this);

            }
            catch (Exception ex)
            {
                log.Error("Exception - " + string.Format("Exception at NPVRRetrieveSeriesRequest. Msg: {0} , Ex Type: {1} , Req: {2} , ST: {3}", ex.Message, ex.GetType().Name, ToString(), ex.StackTrace), ex);
                throw ex;
            }

            return res;
        }
    }
}
