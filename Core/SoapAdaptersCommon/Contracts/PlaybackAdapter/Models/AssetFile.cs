using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    public class AssetFile
    {
        [DataMember]
        public string Url { get; set; }
    }
}