using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Logger;
using StatisticsBL;

namespace Catalog
{
    [Serializable]
    [DataContract]
    public class BuzzMeterRequest : BaseRequest, IRequestImp
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        [DataMember]
        public string m_sKey;


        public BuzzMeterRequest()
            : base()
        {
        }


        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                BuzzMeterRequest request = (BuzzMeterRequest)oBaseRequest;
                BuzzMeterResponse response = new BuzzMeterResponse();

                if (request == null || string.IsNullOrEmpty(request.m_sKey))
                    throw new Exception("request object is null or Required variables is null");

                string sCheckSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID);
                if (sCheckSignature != request.m_sSignature)
                    throw new Exception("Signatures dosen't match");

                BaseStaticticsBL staticticsBL = StatisticsBL.Utils.GetInstance(request.m_nGroupID);
                response.m_buzzAverScore = staticticsBL.GetBuzzAverScore(request.m_sKey);
    
                return (BaseResponse)response;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("BuzzMeterRequest", string.Format("ex={0}", ex.Message), "Catalog");
                throw ex;
            }
        }
    }
}
