using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class LinearMediaAsset: MediaAsset
    {

        public bool EnableCDVR { get; set; }
        public bool EnableCatchUp { get; set; }
        public bool EnableStartOver { get; set; }
        public bool EnableTrickPlay { get; set; }
        public long CatchUpBuffer { get; set; }
        public long TrickPlayBuffer { get; set; }
        public bool EnableRecordingPlaybackNonEntitledChannel { get; set; }
        public long EpgChannelId { get; set; }
    }
}
