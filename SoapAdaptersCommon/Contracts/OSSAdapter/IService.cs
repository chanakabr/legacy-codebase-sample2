using OSSAdapter.Models;
using System.Collections.Generic;
using System.ServiceModel;
using AdapaterCommon.Models;

namespace OSSAdapter
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AdapterStatus SetConfiguration(int ossAdpaterId, List<KeyValue> settings, int partnerId, long timeStamp, string signature);

        [OperationContract]
        HouseholdPaymentGatewayResponse GetHouseholdPaymentGatewaySettings(string householdId, string ip, long timeStamp, string signature);

        [OperationContract]
        EntitlementsResponse GetEntitlements(string userId, long timeStamp, string signature);
    }
}
