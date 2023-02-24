using ApiObjects.MediaMarks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class PlaybackContextResponse
    {
        public int AssetId { get; set; }

        public List<MediaFile> Files { get; set; }
        
        public Response.Status Status { get; set; }

        public DevicePlayData ConcurrencyData { get; set; }
    }

    public class MediaFile
    {
        public long MediaId { get; set; }

        public long Id { get; set; }

        public string Type { get; set; }
        
        public long TypeId { get; set; }

        public string Url { get; set; }

        public string AltUrl { get; set; }

        public string DirectUrl { get; set; }
        
        public string AltDirectUrl { get; set; }

        public long Duration { get; set; }

        public string ExternalId { get; set; }

        public StreamerType? StreamerType { get; set; }

        public bool IsTrailer { get; set; }

        public int CdnId { get; set; }

        public int AltCdnId { get; set; }

        public int DrmId { get; set; }

        public AdsPolicy? AdsPolicy { get; set; }
        
        public string AdsParam { get; set; }

        public string Opl { get; set; }

        public BusinessModuleDetails BusinessModuleDetails { get; set; }

        public long GroupId { get; set; }

        public string Labels { get; set; }

        public IDictionary<string, IEnumerable<string>> DynamicData { get; set; }

        public int GetCdnId(bool isAlternative)
        {
            return !isAlternative ? this.CdnId : this.AltCdnId;
        }
        
        public string GetUrl(bool isAlternative)
        {
            return !isAlternative ? this.Url : this.AltUrl;
        }
    }

    public class BusinessModuleDetails
    {
        public int? BusinessModuleId { get; set; }
        public eTransactionType? BusinessModuleType { get; set; }
    }

    public class PlayManifestResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public string Url { get; set; }
    }
}
