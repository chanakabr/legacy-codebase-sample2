using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using EngagementAdapter.Models;

namespace EngagementAdapter
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AdapterStatus SetConfiguration(int adapterId, string providerUrl, KeyValue[] connectionSettings, int partnerId, long timeStamp, string signature);

        [OperationContract]
        EngagementResponse GetList(int adapterId, string dynamicData, long timeStamp, string signature);
    }
}
