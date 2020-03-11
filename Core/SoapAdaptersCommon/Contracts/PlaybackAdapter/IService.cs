using AdapaterCommon.Models;
using System.ServiceModel;

namespace PlaybackAdapter
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AdapterStatus SetConfiguration(long adapterId, string settings, int partnerId, long timeStamp, string signature);

        [OperationContract]
        PlaybackAdapterResponse GetPlaybackContext(AdapterPlaybackContextOptions adapterPlaybackContextOptions, RequestPlaybackContextOptions requestPlaybackContextOptions);

        [OperationContract]
        PlaybackAdapterResponse GetPlaybackManifest(AdapterPlaybackContextOptions contextOptions, RequestPlaybackContextOptions requestPlaybackContextOptions);

    }
}
