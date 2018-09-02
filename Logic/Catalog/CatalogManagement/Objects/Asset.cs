using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Core.Catalog.CatalogManagement
{
    public class Asset
    {
        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]        
        public long Id { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public eAssetTypes AssetType { get; set; }

        [ExcelTemplateAttribute(PropertyValueRequired = true, SystemName = AssetManager.NAME_META_SYSTEM_NAME)]
        public string Name { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        // Name in other languages other then default (when language="*")        
        public List<LanguageContainer> NamesWithLanguages { get; set; }

        [ExcelTemplateAttribute(PropertyValueRequired = true, SystemName = AssetManager.DESCRIPTION_META_SYSTEM_NAME)]
        public string Description { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        // Description in other languages other then default (when language="*")
        public List<LanguageContainer> DescriptionsWithLanguages { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public DateTime? CreateDate { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public DateTime? UpdateDate { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false, SystemName = AssetManager.PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME)]
        public DateTime? StartDate { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false, SystemName = AssetManager.CATALOG_END_DATE_TIME_META_SYSTEM_NAME)]
        public DateTime? EndDate { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public List<Metas> Metas { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public List<Tags> Tags { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public List<Image> Images { get; set; }

        [ExcelTemplateAttribute(PropertyValueRequired = true, IsKeyProperty = true, SystemName = AssetManager.EXTERNAL_ID_META_SYSTEM_NAME)]
        [XmlElement("co_guid")]
        public string CoGuid{ get; set; }

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
            this.Images = new List<Image>();
            this.CoGuid = string.Empty;            
        }

        public Asset(long id, eAssetTypes assetType, string name, List<LanguageContainer> namesWithLanguages, string description, List<LanguageContainer> descriptionsWithLanguages, DateTime? createDate, 
                    DateTime? startDate, DateTime? updateDate, DateTime? endDate, List<Metas> metas, List<Tags> tags, List<Image> images, string coGuid)
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
            this.Images = images != null ? new List<Image>(images) : new List<Image>();
            this.CoGuid = coGuid;            
        }

        public Asset(Asset assetToCopy)
        {
            this.Id = assetToCopy.Id;
            this.AssetType = assetToCopy.AssetType;
            this.Name = assetToCopy.Name;
            this.NamesWithLanguages = assetToCopy.NamesWithLanguages != null ? new List<LanguageContainer>(assetToCopy.NamesWithLanguages) : new List<LanguageContainer>();
            this.Description = assetToCopy.Description;
            this.DescriptionsWithLanguages = assetToCopy.DescriptionsWithLanguages != null ? new List<LanguageContainer>(assetToCopy.DescriptionsWithLanguages) : new List<LanguageContainer>();
            this.CreateDate = assetToCopy.CreateDate;
            this.UpdateDate = assetToCopy.UpdateDate;
            this.StartDate = assetToCopy.StartDate;
            this.EndDate = assetToCopy.EndDate;
            this.Metas = assetToCopy.Metas != null ? new List<Metas>(assetToCopy.Metas) : new List<Metas>();
            this.Tags = assetToCopy.Tags != null ? new List<Tags>(assetToCopy.Tags) : new List<Tags>();
            this.Images = assetToCopy.Images != null ? new List<Image>(assetToCopy.Images) : new List<Image>();
            this.CoGuid = assetToCopy.CoGuid;
        }

        public Asset(Core.Catalog.Response.MediaObj mediaObj)
        {
            long id;
            if (long.TryParse(mediaObj.AssetId, out id))
            {
                this.Id = id;
            }

            this.AssetType = eAssetTypes.MEDIA;
            this.Name = mediaObj.m_sName;
            this.NamesWithLanguages = mediaObj.Name != null ? mediaObj.Name.ToList() : new List<LanguageContainer>();
            this.Description = mediaObj.m_sDescription;
            this.DescriptionsWithLanguages = mediaObj.Description != null ? mediaObj.Description.ToList() : new List<LanguageContainer>();
            this.CreateDate = mediaObj.m_dCreationDate;
            this.UpdateDate = mediaObj.m_dUpdateDate;
            this.StartDate = mediaObj.m_dStartDate;
            this.EndDate = mediaObj.m_dEndDate;
            this.Metas = mediaObj.m_lMetas != null ? new List<Metas>(mediaObj.m_lMetas) : new List<Metas>();
            this.Tags = mediaObj.m_lTags != null ? new List<Tags>(mediaObj.m_lTags) : new List<Tags>();
            this.Images = new List<Image>();
            this.CoGuid = mediaObj.CoGuid;
        }

    }
}