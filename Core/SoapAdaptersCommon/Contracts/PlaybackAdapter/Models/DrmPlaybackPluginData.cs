using AdapaterCommon.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    [KnownType(typeof(CustomDrmPlaybackPluginData))]
    [KnownType(typeof(FairPlayPlaybackPluginData))]
    public class DrmPlaybackPluginData : PluginData
    {
        [DataMember]
        public DrmSchemeName Scheme { get; set; }

        [DataMember]
        public string LicenseURL { get; set; }
        
        [DataMember]
        public List<KeyValue> DynamicData { get; set; }
    }
}