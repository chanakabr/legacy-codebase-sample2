using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    public class AdapterPlaybackContext
    {
        [DataMember]
        public List<PlaybackSource> Sources { get; set; }

        [DataMember]
        public List<RuleAction> Actions { get; set; }

        [DataMember]
        public List<AccessControlMessage> Messages { get; set; }

        [DataMember]
        public List<CaptionPlaybackPluginData> PlaybackCaptions { get; set; }

        [DataMember]
        public List<PlaybackPluginData> Plugins { get; set; }

    }
}