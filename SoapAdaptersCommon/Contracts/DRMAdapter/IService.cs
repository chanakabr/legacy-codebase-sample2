using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using AdapaterCommon.Models;

namespace DRMAdapter
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AdapterStatus SetConfiguration(int adapterId, string settings, int partnerId, long timeStamp, string signature);

        [OperationContract]
        DrmAdapterResponse GetAssetLicenseData(int adapterId, int partnerId, string userId, string assetId, AssetType assetType, long fileId, string externalFileId, string ip, 
            string udid, ContextType context, string recordingId, long timeStamp, string signature);

        [OperationContract]
        DrmAdapterResponse GetDeviceLicenseData(int adapterId, int partnerId, string userId, string udid, string deviceFamily, int deviceBrandId, string ip, long timeStamp, string signature);
    }
}
