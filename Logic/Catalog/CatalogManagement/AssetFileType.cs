using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class AssetFileType
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }
        public bool IsTrailer { get; set; }
        public StreamerType? StreamerType { get; set; }
        public int? DrmId { get; set; }

        public AssetFileType()
        {
            this.Id = 0;
            this.Description = string.Empty;
            this.IsActive = true;
            this.CreateDate = 0;
            this.UpdateDate = 0;
            this.IsTrailer = false;
            this.StreamerType = null;
            this.DrmId = null;
        }
        
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("Id: {0}, ", Id));
            sb.AppendFormat("Description: {0},", Description);
            sb.AppendFormat("IsActive: {0}, ", IsActive);
            sb.AppendFormat("CreateDate: {0}, ", CreateDate);
            sb.AppendFormat("UpdateDate: {0}", UpdateDate);
            sb.AppendFormat("IsTrailer: {0}, ", IsTrailer);
            sb.AppendFormat("StreamerType: {0}, ", StreamerType.HasValue ? StreamerType.Value.ToString() : string.Empty);
            sb.AppendFormat("DrmId: {0}, ", DrmId.HasValue ? DrmId.Value.ToString() : string.Empty);
            return sb.ToString();
        }

    }
}
