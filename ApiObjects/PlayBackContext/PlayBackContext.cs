using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class PlayBackContextResponse
    {
        public int AssetId { get; set; }

        public List<MediaFile> Files { get; set; }

        public List<PlaybackStatus> PlaybackStatuses { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }

    public class MediaFile
    {
        public long MediaId { get; set; }

        public long Id { get; set; }

        public string Type { get; set; }

        public string Url { get; set; } // tan tan

        public long Duration { get; set; }

        public string ExternalId { get; set; }

        public StreamerType? StreamerType { get; set; }

        public bool IsTrailer { get; set; }
    }

    public class PlaybackStatus
    {
        public string Code { get; set; }

        public string Message { get; set; }
    }
}
