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
        public string DeviceRule { get; set; }
        public string GeoBlockRule { get; set; }
        public List<FileMedia> Files { get; set; }
        public string UserTypes { get; set; }

        public MediaAsset()
            :base()
        {
            this.CatalogStartDate = null;
            this.FinalEndDate = null;
            this.MediaType = new MediaType();
            this.EntryId = string.Empty;            
            this.DeviceRule = string.Empty;
            this.GeoBlockRule = string.Empty;
            this.Files = new List<FileMedia>();
            this.UserTypes = string.Empty;
        }

        public MediaAsset(long id, eAssetTypes assetType, string name, List<LanguageContainer> namesWithLanguages, string description, List<LanguageContainer> descriptionsWithLanguages, DateTime createDate, DateTime updateDate,
                        DateTime startDate, DateTime endDate, List<Metas> metas, List<Tags> tags, List<Picture> pictures, string coGuid, bool isActive, DateTime catalogStartDate, DateTime finalEndDate,
                        MediaType mediaType, string entryId, string deviceRule, string geoBlockRule, List<FileMedia> files, string userTypes)
            : base(id, assetType, name, namesWithLanguages, description, descriptionsWithLanguages, createDate, updateDate, startDate, endDate, metas, tags, pictures, coGuid, isActive)
        {
            this.CatalogStartDate = catalogStartDate;
            this.FinalEndDate = finalEndDate;
            this.MediaType = new MediaType(mediaType.m_sTypeName, mediaType.m_nTypeID);
            this.EntryId = entryId;
            this.DeviceRule = deviceRule;
            this.GeoBlockRule = geoBlockRule;
            this.Files = files != null ? new List<FileMedia>(files) : new List<FileMedia>();
            this.UserTypes = userTypes;
        }

    }
}
