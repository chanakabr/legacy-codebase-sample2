using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Catalog
{
    public class AssetStructMeta
    {
        public long AssetStructId { get; set; }
        public long MetaId { get; set; }
        public string IngestReferencePath { get; set; }
        public bool? ProtectFromIngest { get; set; }
        public string DefaultIngestValue { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }
        public bool? IsInherited { get; set; }
        public bool? IsLocationTag { get; set; }
        public int? SuppressedOrder { get; set; } 

        public AssetStructMeta()
        {
            this.AssetStructId = 0;
            this.MetaId = 0;
            this.IngestReferencePath = string.Empty;
            this.ProtectFromIngest = null;
            this.DefaultIngestValue = string.Empty;
            this.CreateDate = 0;
            this.UpdateDate = 0;
            this.IsLocationTag = false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("AssetStructId: {0}, ", AssetStructId));
            sb.AppendFormat("MetaId: {0}, ", MetaId);
            sb.AppendFormat("IngestReferencePath: {0}, ", IngestReferencePath);
            sb.AppendFormat("ProtectFromIngest: {0}, ", ProtectFromIngest);
            sb.AppendFormat("DefaultIngestValue: {0}, ", DefaultIngestValue);
            sb.AppendFormat("CreateDate: {0} ", CreateDate);
            sb.AppendFormat("UpdateDate: {0} ", UpdateDate);
            sb.AppendFormat("IsInherited: {0} ", IsInherited);
            sb.AppendFormat("IsLocationTag: {0} ", IsLocationTag);
            sb.AppendFormat("SuppressedOrder: {0} ", SuppressedOrder ?? -1);
            return sb.ToString();
        }
    }
}