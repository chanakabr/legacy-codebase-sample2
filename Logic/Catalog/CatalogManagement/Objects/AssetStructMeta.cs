using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
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
        public InheritancePolicy? ParentInheritancePolicy { get; set; }
        public IngestInheritancePolicy? IngestPolicy { get; set; }
        public bool? IsInherited { get; set; }

        public AssetStructMeta()
        {
            this.AssetStructId = 0;
            this.MetaId = 0;
            this.IngestReferencePath = string.Empty;
            this.ProtectFromIngest = null;
            this.DefaultIngestValue = string.Empty;
            this.CreateDate = 0;
            this.UpdateDate = 0;         
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
            sb.AppendFormat("InheritancePolicy: {0} ", ParentInheritancePolicy);            
            sb.AppendFormat("IngestInheritancePolicy: {0} ", IngestPolicy);            
            return sb.ToString();
        }
    }

    public enum InheritancePolicy
    {
        Add = 0,
        Replace = 1
    }

    public enum IngestInheritancePolicy
    {
        Add = 0,
        Replace = 1
    }
}
