using System.Collections.Generic;

namespace ApiObjects.PlaybackAdapter
{

    public class PlaybackSource : MediaFile
    {
        public string Format { get; set; }

        public string Protocols { get; set; }

        public List<DrmPlaybackPluginData> Drm { get; set; }

        public AdsPolicy? AdsPolicy { get; set; }

        public string AdsParams { get; set; }

        public string FileExtention { get; set; }

        public int DrmId;

        public bool IsTokenized;
    }
}