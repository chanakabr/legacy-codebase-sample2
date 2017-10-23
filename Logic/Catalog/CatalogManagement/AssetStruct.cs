using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class AssetStruct
    {
        public long Id { get; set; }
        public LanguageContainer[] Names { get; set; }        
        public string SystemName { get; set; }
        public List<long> MetaIds { get; set; }
        public bool? IsPredefined { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }

        public AssetStruct()
        {
            this.Id = 0;
            this.Names = new LanguageContainer[0];
            this.SystemName = string.Empty;
            this.MetaIds = new List<long>();
            this.IsPredefined = null;
            this.CreateDate = 0;
            this.UpdateDate = 0;            
        }

        public AssetStruct(long id, string name, string systemName, bool isPredefined, long createDate, long updateDate)
        {
            this.Id = id;
            //TODO: Lior - init languageContainer, currently using "eng" but its wrong to do that
            this.Names = new LanguageContainer[1] { new LanguageContainer("eng", name) };
            this.SystemName = systemName;
            this.MetaIds = new List<long>();
            this.IsPredefined = isPredefined;
            this.CreateDate = createDate;
            this.UpdateDate = updateDate;            
        }

        public AssetStruct(AssetStruct assetStructToCopy)
        {
            this.Id = assetStructToCopy.Id;
            this.Names = new List<LanguageContainer>(assetStructToCopy.Names).ToArray();
            this.SystemName = string.Copy(assetStructToCopy.SystemName);
            this.MetaIds = new List<long>(assetStructToCopy.MetaIds);
            this.IsPredefined = assetStructToCopy.IsPredefined;
            this.CreateDate = assetStructToCopy.CreateDate;
            this.UpdateDate = assetStructToCopy.UpdateDate;            
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("Id: {0}, ", Id));
            sb.AppendFormat("Names: {0}, ", Names != null && Names.Length > 0 ? string.Join(",", Names.Select(x => string.Format("languageCode: {0}, value: {1}",
                                                                                                 x.m_sLanguageCode3, x.m_sValue)).ToList()) : string.Empty);
            sb.AppendFormat("SystemName: {0}, ", SystemName);
            sb.AppendFormat("MetaIds: {0}, ", MetaIds != null ? string.Join(",", MetaIds) : string.Empty);
            sb.AppendFormat("IsPredefined: {0}, ", IsPredefined.HasValue ? IsPredefined.Value.ToString() : string.Empty);
            sb.AppendFormat("CreateDate: {0}, ", CreateDate);
            sb.AppendFormat("UpdateDate: {0}", UpdateDate);            
            return sb.ToString();
        }

    }
}
