using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.NewCatalogManagement
{
    public class AssetStruct
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string SystemName { get; set; }
        public List<long> MetaIds { get; set; }
        public bool  IsPredefined { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }

        public AssetStruct()
        {
            this.Id = 0;
            this.Name = string.Empty;
            this.SystemName = string.Empty;
            this.MetaIds = new List<long>();
            this.IsPredefined = false;
            this.CreateDate = 0;
            this.UpdateDate = 0;
        }

        public AssetStruct(long id, string name, string systemName, bool isPredefined, long createDate, long updateDate)
        {
            this.Id = id;
            this.Name = name;
            this.SystemName = systemName;
            this.MetaIds = new List<long>();
            this.IsPredefined = isPredefined;
            this.CreateDate = createDate;
            this.UpdateDate = updateDate;
        }

    }
}
