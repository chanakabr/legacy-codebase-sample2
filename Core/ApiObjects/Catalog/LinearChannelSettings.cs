using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Catalog
{
    public class LinearChannelSettings
    {
        public string ChannelID { get; set; }

        public string ChannelExternalID { get; set; }

        public bool EnableCDVR { get; set; }

        public bool EnableCatchUp { get; set; }

        public bool EnableStartOver { get; set; }

        public bool EnableTrickPlay { get; set; }

        public long CatchUpBuffer { get; set; }

        public long TrickPlayBuffer { get; set; }

        public bool EnableRecordingPlaybackNonEntitledChannel { get; set; }

        public bool EnableRecordingPlaybackNonExistingChannel { get; set; }

        public long LinearMediaId { get; set; }
    }
}
