using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class LinearMediaAsset: MediaAsset
    {
        public long EpgChannelId { get; set; }
        public TstvState? EnableCdvr { get; set; }
        public TstvState? EnableCatchUp { get; set; }
        public TstvState? EnableStartOver { get; set; }
        public TstvState? EnableTrickPlay { get; set; }
        public TstvState? EnableRecordingPlaybackNonEntitledChannel { get; set; }
        public long? CatchUpBuffer { get; set; }
        public long? TrickPlayBuffer { get; set; }
        public string ExternalIngestId { get; set; }
        public string ExternalCdvrId { get; set; }
        public bool CdvrEnabled { get; set; }
        public bool CatchUpEnabled { get; set; }
        public bool StartOverEnabled { get; set; }
        public bool TrickPlayEnabled { get; set; }
        public long BufferCatchUp { get; set; }
        public long BufferTrickPlay { get; set; }
        public bool RecordingPlaybackNonEntitledChannelEnabled { get; set; }

        public LinearMediaAsset()
            :base()
        {
            this.EpgChannelId = 0;
            this.EnableCdvr = null;
            this.EnableCatchUp = null;
            this.EnableStartOver = null;
            this.EnableTrickPlay = null;
            this.EnableRecordingPlaybackNonEntitledChannel = null;
            this.CatchUpBuffer = null;
            this.TrickPlayBuffer = null;
            this.ExternalIngestId = null;
            this.ExternalCdvrId = null;
            this.CdvrEnabled = false;
            this.CatchUpEnabled = false;
            this.StartOverEnabled = false;
            this.TrickPlayEnabled = false;
            this.BufferCatchUp = 0;
            this.BufferTrickPlay = 0;
            this.RecordingPlaybackNonEntitledChannelEnabled = false;
            this.MediaAssetType = MediaAssetType.Linear;
        }

    }
}
