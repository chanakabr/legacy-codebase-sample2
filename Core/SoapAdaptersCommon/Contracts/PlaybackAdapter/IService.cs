using System.ServiceModel;
using SoapAdaptersCommon.Contracts.PlaybackAdapter.Models;
using AdapterStatus = AdapaterCommon.Models.AdapterStatus;

namespace PlaybackAdapter
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AdapterStatus SetConfiguration(long adapterId, string settings, int partnerId, long timeStamp, string signature);
        
        [OperationContract]
        PlaybackImplementationsResponse GetConfiguration(int adapterId);

        [OperationContract]
        PlaybackAdapterResponse GetPlaybackContext(AdapterPlaybackContextOptions adapterPlaybackContextOptions, RequestPlaybackContextOptions requestPlaybackContextOptions);

        [OperationContract]
        PlaybackAdapterResponse GetPlaybackManifest(AdapterPlaybackContextOptions contextOptions, RequestPlaybackContextOptions requestPlaybackContextOptions);

        [OperationContract]
        ConcurrencyCheckResponse ConcurrencyCheck(ConcurrencyCheckRequest request);

    }
}
