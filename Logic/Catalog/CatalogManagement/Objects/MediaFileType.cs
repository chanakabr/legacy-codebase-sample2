using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class MediaFileType
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }
        public bool IsTrailer { get; set; }
        public StreamerType? StreamerType { get; set; }
        public int? DrmId { get; set; }
        public MediaFileTypeQuality Quality { get; set; }
        public HashSet<string> VideoCodecs { get; set; }
        public HashSet<string> AudioCodecs { get; set; }

        public MediaFileType()
        {
            this.Id = 0;
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.IsActive = true;
            this.CreateDate = 0;
            this.UpdateDate = 0;
            this.IsTrailer = false;
            this.StreamerType = null;
            this.DrmId = null;
            this.Quality = MediaFileTypeQuality.None;
            this.VideoCodecs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.AudioCodecs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("Id: {0}, ", Id));            
            sb.AppendFormat("Name: {0},", string.IsNullOrEmpty(Name) ? string.Empty : Name);
            sb.AppendFormat("Description: {0},", string.IsNullOrEmpty(Description) ? string.Empty : Description);
            sb.AppendFormat("IsActive: {0}, ", IsActive);
            sb.AppendFormat("CreateDate: {0}, ", CreateDate);
            sb.AppendFormat("UpdateDate: {0}", UpdateDate);
            sb.AppendFormat("IsTrailer: {0}, ", IsTrailer);
            sb.AppendFormat("StreamerType: {0}, ", StreamerType.HasValue ? StreamerType.Value.ToString() : string.Empty);
            sb.AppendFormat("DrmId: {0}, ", DrmId.HasValue ? DrmId.Value.ToString() : string.Empty);
            sb.AppendFormat("Quality: {0}, ", Quality.ToString());
            sb.AppendFormat("VideoCodecs: {0},", VideoCodecs == null || VideoCodecs.Count > 0 ? string.Empty : string.Join(",", VideoCodecs));
            sb.AppendFormat("AudioCodecs: {0},", AudioCodecs == null || AudioCodecs.Count > 0 ? string.Empty : string.Join(",", AudioCodecs));
            return sb.ToString();
        }

        public string CreateMappedHashSetForKalturaMediaFileType(HashSet<string> codecs)
        {
            if (codecs != null && codecs.Count > 0)
            {
                return string.Join(",", codecs);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
