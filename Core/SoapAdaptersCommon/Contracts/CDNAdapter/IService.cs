using CDNAdapter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using AdapaterCommon.Models;

namespace CDNAdapter
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AdapterStatus SetConfiguration(int adapterId, List<KeyValue> settings, int partnerId, long timeStamp, string signature);

        [OperationContract]
        LinkResponse GetVodLink(int adapterId, string userId, string url, string fileType, int assetId, int contentId, string ip, long timeStamp, string signature);

        [OperationContract]
        LinkResponse GetRecordingLink(int adapterId, string userId, string url, string fileType, string recordingId, string ip, long timeStamp, string signature);
        
        [OperationContract]
        LinkResponse GetEpgLink(int adapterId, string userId, string url, string fileType, int programId, int assetId, int contentId, long startTimeSeconds, int actionType, string ip, long timeStamp, string signature);
    }
}
