using ApiObjects;
using NPVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    [DataContract]
    public class NPVRRetrieveSeriesRequest : BaseRequest, IRequestImp
    {
        [DataMember]
        public RecordedEPGOrderObj m_oOrderObj;

        protected override void CheckRequestValidness()
        {
            if (!NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(m_nGroupID))
            {
                throw new ArgumentException(String.Concat("Group: ", m_nGroupID, " does not have NPVR implementation."));
            }
        }

        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                CheckRequestValidness();
                CheckSignature(this);

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", string.Format("Exception at NPVRRetrieveSeriesRequest. Msg: {0} , Ex Type: {1} , Req: {2} , ST: {3}", ex.Message, ex.GetType().Name, ToString(), ex.StackTrace), "NPVRRetrieveSeriesRequest");
                throw ex;
            }
        }
    }
}
