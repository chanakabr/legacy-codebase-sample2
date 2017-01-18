using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Core.Catalog.Response;
using KLogMonitor;
using StatisticsBL;

namespace Core.Catalog.Request
{
    [Serializable]
    [DataContract]
    public class BuzzMeterRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public string m_sKey;


        public BuzzMeterRequest()
            : base()
        {
        }


        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                BuzzMeterRequest request = oBaseRequest as BuzzMeterRequest;
                BuzzMeterResponse response = new BuzzMeterResponse();

                if (request == null || string.IsNullOrEmpty(request.m_sKey))
                    throw new Exception("request object is null or Required variables is null");

                CheckSignature(request);

                BaseStaticticsBL staticticsBL = StatisticsBL.Utils.GetInstance(request.m_nGroupID);
                response.m_buzzAverScore = staticticsBL.GetBuzzAverScore(request.m_sKey);

                return response;
            }
            catch (Exception ex)
            {
                log.Error("BuzzMeterRequest - " + string.Format("ex={0}", ex.Message), ex);
                throw ex;
            }
        }
    }
}
