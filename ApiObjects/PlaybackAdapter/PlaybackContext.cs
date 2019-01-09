using System.Collections.Generic;

namespace ApiObjects.PlaybackAdapter
{
    public class PlaybackContext
    {
        public List<PlaybackSource> Sources { get; set; }

        public List<RuleAction> Actions { get; set; }

        public List<AccessControlMessage> Messages { get; set; }

        public List<CaptionPlaybackPluginData> PlaybackCaptions { get; set; }

        public List<BumpersPlaybackPluginData> PlaybackBumpers { get; set; }
    }
}
