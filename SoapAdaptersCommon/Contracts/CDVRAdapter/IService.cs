using CDVRAdapter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using AdapaterCommon.Models;

namespace CDVRAdapter
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AdapterStatus SetConfiguration(int adapterId, List<KeyValue> settings, int partnerId, long timeStamp, string signature);

        [OperationContract]
        RecordingResponse Record(long startTimeSeconds, long durationSeconds, string epgId, string channelId, List<long> domainIds, int adapterId, long timeStamp, string signature);

        [OperationContract]
        RecordingResponse GetRecordingStatus(string recordingId, string channelId, int adapterId, long timeStamp, string signature);
        
        [OperationContract]
        RecordingResponse UpdateRecordingSchedule(string recordingId, string epgId, string channelId, int adapterId, long startDateSeconds, long durationSeconds, long timeStamp, string signature);

        [OperationContract]
        RecordingResponse CancelRecording(string recordingId, string epgId, string channelId, long domainId, int adapterId, long timeStamp, string signature);

        [OperationContract]
        RecordingResponse DeleteRecording(string recordingId, string epgId, string channelId, List<long> domainIds, int adapterId, long timeStamp, string signature);

        [OperationContract]
        RecordingResponse GetRecordingLinks(string recordingId, string channelId, long domainId, int adapterId, long timeStamp, string signature);

        [OperationContract]
        ExternalRecordingResponse GetCloudRecording(string userId, long domainId, List<KeyValue> adapterData, int adapterId, long timeStamp, string signature);

        [OperationContract]
        ExternalSeriesRecordingResponse GetCloudSeriesRecording(string userId, long domainId, List<KeyValue> adapterData, int adapterId, long timeStamp, string signature);
    }
}
