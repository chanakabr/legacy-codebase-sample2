using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    [DataContract]
    public class NPVRRetrieveRequest : BaseRequest, IRequestImp
    {
        public BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            NPVRRetrieveResponse res = new NPVRRetrieveResponse();
            try
            {

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", string.Format("Exception at GetResponse. Req: {0} , Ex Msg: {1} , Ex Type: {2} , ST: {3}", ToString(), ex.Message, ex.GetType().Name, ex.StackTrace), "NPVRRetrieveRequest");
                throw ex;
            }

            return res;
        }
    }
}
