using System.Runtime.Serialization;
using System.Xml.Serialization;

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
    }
}