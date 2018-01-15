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
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("Id: {0}, ", Id));
            sb.AppendFormat("Name: {0},", Name);
            sb.AppendFormat("Description: {0},", Description);
            sb.AppendFormat("IsActive: {0}, ", IsActive);
            sb.AppendFormat("CreateDate: {0}, ", CreateDate);
            sb.AppendFormat("UpdateDate: {0}", UpdateDate);
            sb.AppendFormat("IsTrailer: {0}, ", IsTrailer);
            sb.AppendFormat("StreamerType: {0}, ", StreamerType.HasValue ? StreamerType.Value.ToString() : string.Empty);
            sb.AppendFormat("DrmId: {0}, ", DrmId.HasValue ? DrmId.Value.ToString() : string.Empty);
            sb.AppendFormat("Quality: {0}, ", Quality.ToString());
            return sb.ToString();
        }

    }
}
