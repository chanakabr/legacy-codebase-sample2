using AdapaterCommon.Models;
using SoapAdaptersCommon.Contracts.SmsAdapter.Models;
using System.Collections.Generic;
using System.ServiceModel;

namespace SoapAdaptersCommon.Contracts.SmsAdapter
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AdapterStatus SetConfiguration(int adapterId, int partnerId, List<KeyValue> configuration, string signature);

        [OperationContract]
        bool Send(int adapterId, int partnerId, SendSmsRequestModel model);
    }
}
