using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog
{
    public class LinearChannelSettings
    {
        public string ChannelID;

        public bool EnableCDVR;
     
        public bool EnableCatchUp;
     
        public bool EnableStartOver;
     
        public bool EnableTrickPlay;

        public long CatchUpBuffer;
     
        public long TrickPlayBuffer;

        public bool EnableRecordingPlaybackNonEntitledChannel;

        public bool EnableRecordingPlaybackNonExistingChannel;

        public long linearMediaId;
    }
}
