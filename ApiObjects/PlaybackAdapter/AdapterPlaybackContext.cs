using System.Collections.Generic;

namespace ApiObjects.PlaybackAdapter
{
    public class AdapterPlaybackContext
    {
        public List<PlaybackSource> Sources { get; set; }

        public List<RuleAction> Actions { get; set; }

        public List<AccessControlMessage> Messages { get; set; }
    }
}
