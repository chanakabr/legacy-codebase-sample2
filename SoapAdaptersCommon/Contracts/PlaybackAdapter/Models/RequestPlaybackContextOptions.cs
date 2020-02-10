using AdapaterCommon.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    public class RequestPlaybackContextOptions
    {
        [DataMember]
        public string MediaProtocol { get; set; }
        [DataMember]
        public string StreamerType { get; set; }
        [DataMember]
        public string AssetFileIds { get; set; }
        [DataMember]
        public List<KeyValue> AdapterData { get; set; }
        [DataMember]
        public AdapterPlaybackContextType? Context { get; set; }
        [DataMember]
        public AdapterUrlType UrlType { get; set; }
        [DataMember]
        public string AssetId { get; set; }
        [DataMember]
        public AdapterAssetType AssetType { get; set; }
    }

    public enum AdapterAssetType
    {
        media,
        recording,
        epg
    }

    public enum AdapterPlaybackContextType
    {
        TRAILER,
        CATCHUP,
        START_OVER,
        PLAYBACK,
        DOWNLOAD
    }

    public enum AdapterUrlType
    {
        PLAYMANIFEST,
        DIRECT
    }
}