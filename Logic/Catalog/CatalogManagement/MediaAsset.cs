using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class MediaAsset : Asset
    {
        
        public DateTime? CatalogStartDate { get; set; }
        public DateTime? FinalEndDate { get; set; }
        public MediaType MediaType { get; set; }
        public string EntryId { get; set; }        
        public int? DeviceRuleId { get; set; }
        public int? GeoBlockRuleId { get; set; }
        public List<FileMedia> Files { get; set; }
        public string UserTypes { get; set; }
        public bool? IsActive { get; set; }

        public MediaAsset()
            :base()
        {
            this.CatalogStartDate = null;
            this.FinalEndDate = null;
            this.MediaType = new MediaType();
            this.EntryId = string.Empty;            
            this.DeviceRuleId = null;
            this.GeoBlockRuleId = null;
            this.Files = new List<FileMedia>();
            this.UserTypes = string.Empty;
            this.IsActive = null;
        }

        public MediaAsset(long id, eAssetTypes assetType, string name, List<LanguageContainer> namesWithLanguages, string description, List<LanguageContainer> descriptionsWithLanguages, DateTime? createDate,
                        DateTime? updateDate, DateTime? startDate, DateTime? endDate, List<Metas> metas, List<Tags> tags, List<Picture> pictures, string coGuid, bool isActive, DateTime? catalogStartDate,
                        DateTime? finalEndDate, MediaType mediaType, string entryId, int? deviceRuleId, int? geoBlockRuleId, List<FileMedia> files, string userTypes)
            : base(id, assetType, name, namesWithLanguages, description, descriptionsWithLanguages, createDate, updateDate, startDate, endDate, metas, tags, pictures, coGuid)
        {
            this.CatalogStartDate = catalogStartDate;
            this.FinalEndDate = finalEndDate;
            this.MediaType = new MediaType(mediaType.m_sTypeName, mediaType.m_nTypeID);
            this.EntryId = entryId;
            this.DeviceRuleId = deviceRuleId;
            this.GeoBlockRuleId = geoBlockRuleId;
            this.Files = files != null ? new List<FileMedia>(files) : new List<FileMedia>();
            this.UserTypes = userTypes;
            this.IsActive = IsActive;
        }

    }
}
