using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class Asset
    {
        public long Id { get; set; }
        public eAssetTypes AssetType { get; set; }
        public string Name { get; set; }
        // Name in other languages other then default (when language="*")
        public List<LanguageContainer> NamesWithLanguages { get; set; }
        public string Description { get; set; }
        // Description in other languages other then default (when language="*")
        public List<LanguageContainer> DescriptionsWithLanguages { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? StartDate { get; set; }        
        public DateTime? EndDate { get; set; }
        public List<Metas> Metas { get; set; }
        public List<Tags> Tags { get; set; }
        public Dictionary<string, int> TagNameToIdMap { get; set; }
        public List<Picture> Pictures { get; set; }
        public string CoGuid{ get; set; }
        public bool? IsActive{ get; set; }

        public Asset()
        {
            this.Id = 0;
            this.AssetType = eAssetTypes.UNKNOWN;
            this.Name = string.Empty;
            this.NamesWithLanguages = new List<LanguageContainer>();
            this.Description = string.Empty;
            this.DescriptionsWithLanguages = new List<LanguageContainer>();
            this.CreateDate = null;
            this.UpdateDate = null;
            this.StartDate = null;
            this.EndDate = null;
            this.Metas = new List<Metas>();
            this.Tags = new List<Tags>();
            this.TagNameToIdMap = new Dictionary<string, int>();
            this.Pictures = new List<Picture>();
            this.CoGuid = string.Empty;
            this.IsActive = null;
        }

        public Asset(long id, eAssetTypes assetType, string name, List<LanguageContainer> namesWithLanguages, string description, List<LanguageContainer> descriptionsWithLanguages, DateTime? createDate, 
                    DateTime? startDate, DateTime? updateDate, DateTime? endDate, List<Metas> metas, List<Tags> tags, List<Picture> pictures, string coGuid, bool isActive, Dictionary<string, int> tagNameToIdMap = null)
        {
            this.Id = id;
            this.AssetType = assetType;
            this.Name = name;
            this.NamesWithLanguages = namesWithLanguages != null ? new List<LanguageContainer>(namesWithLanguages) : new List<LanguageContainer>();
            this.Description = description;
            this.DescriptionsWithLanguages = descriptionsWithLanguages != null ? new List<LanguageContainer>(descriptionsWithLanguages) : new List<LanguageContainer>();
            this.CreateDate = createDate;
            this.UpdateDate = updateDate;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Metas = metas != null ? new List<Metas>(metas) : new List<Metas>();
            this.Tags = tags != null ? new List<Tags>(tags) : new List<Tags>();
            this.Pictures = pictures != null ? new List<Picture>(pictures) : new List<Picture>();
            this.CoGuid = coGuid;
            this.IsActive = isActive;
            this.TagNameToIdMap = tagNameToIdMap != null ? new Dictionary<string, int>(tagNameToIdMap, StringComparer.OrdinalIgnoreCase) : new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        }

    }
}