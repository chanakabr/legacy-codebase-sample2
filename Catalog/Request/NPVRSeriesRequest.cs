using ApiObjects;
using NPVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Catalog.Response;
using KLogMonitor;
using System.Reflection;

namespace Catalog.Request
{
    [DataContract]
    public class NPVRSeriesRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public RecordedEPGOrderObj m_oOrderObj;

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            NPVRSeriesResponse res = new NPVRSeriesResponse();

            try
            {
                CheckRequestValidness();
                CheckSignature(this);

                INPVRProvider npvr;
                if (!NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(m_nGroupID, out npvr))
                {
                    throw new ArgumentException(String.Concat("Group: ", m_nGroupID, " does not have NPVR implementation."));
                }

                res = Catalog.GetSeriesRecordings(m_nGroupID, this, npvr);

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
