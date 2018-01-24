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
        public List<AssetFile> Files { get; set; }
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
            this.Files = new List<AssetFile>();
            this.UserTypes = string.Empty;
            this.IsActive = null;
        }

        public MediaAsset(long id, eAssetTypes assetType, string name, List<LanguageContainer> namesWithLanguages, string description, List<LanguageContainer> descriptionsWithLanguages,
                        DateTime? createDate, DateTime? updateDate, DateTime? startDate, DateTime? endDate, List<Metas> metas, List<Tags> tags, List<Image> images, string coGuid, bool isActive,
                        DateTime? catalogStartDate, DateTime? finalEndDate, MediaType mediaType, string entryId, int? deviceRuleId, int? geoBlockRuleId, List<AssetFile> files, string userTypes)
            : base(id, assetType, name, namesWithLanguages, description, descriptionsWithLanguages, createDate, startDate, updateDate, endDate, metas, tags, images, coGuid)
        {
            this.CatalogStartDate = catalogStartDate;
            if (this.CatalogStartDate.HasValue)
            {                                
                this.Metas.Add(new Metas(new TagMeta(AssetManager.CATALOG_START_DATE_TIME_META_SYSTEM_NAME,MetaType.DateTime.ToString()), this.CatalogStartDate.Value.ToString(), null));
            }

            this.FinalEndDate = finalEndDate;
            if (this.FinalEndDate.HasValue)
            {
                this.Metas.Add(new Metas(new TagMeta(AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, MetaType.DateTime.ToString()), this.FinalEndDate.Value.ToString(), null));
            }

            this.MediaType = new MediaType(mediaType.m_sTypeName, mediaType.m_nTypeID);
            this.EntryId = entryId;
            this.DeviceRuleId = deviceRuleId;
            this.GeoBlockRuleId = geoBlockRuleId;
            this.Files = files != null ? new List<AssetFile>(files) : new List<AssetFile>();
            this.UserTypes = userTypes;
            this.IsActive = isActive;
        }

    }
}
