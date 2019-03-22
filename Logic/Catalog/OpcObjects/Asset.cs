using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TVinciShared;

namespace Core.Catalog 
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class Asset : IExcelObject
    {
        #region Consts

        public virtual string DistributedTask { get { return null; } }
        public virtual string RoutingKey { get { return null; } }

        public const string EXTERNAL_ASSET_ID = "External Asset ID ";
        public const string METAS = "METAS";
        public const string TAGS = "TAGS";
        public const string IMAGES = "Image URL";

        #endregion

        #region Data Members    

        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("AssetType")]
        public eAssetTypes AssetType { get; set; }

        [ExcelColumn(ExcelColumnType.Meta, AssetManager.NAME_META_SYSTEM_NAME)]
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "NamesWithLanguages",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        /// <summary>
        /// Name in other languages other then default (when language="*")        
        /// </summary>
        public List<LanguageContainer> NamesWithLanguages { get; set; }

        [ExcelColumn(ExcelColumnType.Meta, AssetManager.DESCRIPTION_META_SYSTEM_NAME)]
        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "DescriptionsWithLanguages",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        /// <summary>
        /// Description in other languages other then default (when language="*")
        /// </summary>
        public List<LanguageContainer> DescriptionsWithLanguages { get; set; }

        [JsonProperty("CreateDate")]
        public DateTime? CreateDate { get; set; }

        [JsonProperty("UpdateDate")]
        public DateTime? UpdateDate { get; set; }

        [ExcelColumn(ExcelColumnType.AvailabilityMeta, AssetManager.PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME, IsMandatory =true, IsUniqueMeta = true)]
        [JsonProperty("StartDate")]
        public DateTime? StartDate { get; set; }

        [ExcelColumn(ExcelColumnType.AvailabilityMeta, AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, IsMandatory = true, IsUniqueMeta = true)]
        [JsonProperty("EndDate")]
        public DateTime? EndDate { get; set; }

        [ExcelColumn(ExcelColumnType.Meta, METAS)]
        [JsonProperty(PropertyName = "Metas",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<Metas> Metas { get; set; }

        [ExcelColumn(ExcelColumnType.Tag, TAGS)]
        [JsonProperty(PropertyName = "Tags",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<Tags> Tags { get; set; }

        [ExcelColumn(ExcelColumnType.Image, IMAGES)]
        [JsonProperty(PropertyName = "Images",
                      TypeNameHandling = TypeNameHandling.Auto,
                      ItemTypeNameHandling = TypeNameHandling.Auto,
                      ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<Image> Images { get; set; }

        [ExcelColumn(ExcelColumnType.Basic, Asset.EXTERNAL_ASSET_ID, IsMandatory = true)]
        [XmlElement("co_guid")]
        [JsonProperty("CoGuid")]
        public string CoGuid { get; set; }
        
        #endregion

        #region Ctor's

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
            this.Metas = mediaObj.m_lMetas != null ? ConvertMediaObjMetasToAssetMetas(mediaObj.m_lMetas) : new List<Metas>();
            this.Tags = mediaObj.m_lTags != null ? new List<Tags>(mediaObj.m_lTags) : new List<Tags>();
            this.Tags.ForEach(x => x.m_oTagMeta.m_sType = MetaType.Tag.ToString());
            this.Images = new List<Image>();
            this.CoGuid = mediaObj.CoGuid;
        }

        #endregion
        
        private List<Metas> ConvertMediaObjMetasToAssetMetas(List<Metas> metas)
        {
            List<Metas> result = new List<Catalog.Metas>();
            foreach (Metas meta in metas)
            {                
                Metas metaToAdd = new Catalog.Metas() { m_oTagMeta = new TagMeta(meta.m_oTagMeta.m_sName, meta.m_oTagMeta.m_sType), m_sValue = meta.m_sValue, Value = meta.Value };
                string currentMetaTypeLowered = meta.m_oTagMeta.m_sType.ToLower();
                if (currentMetaTypeLowered == typeof(bool).ToString().ToLower())
                {
                    metaToAdd.m_oTagMeta.m_sType = ApiObjects.MetaType.Bool.ToString();
                }
                else if (currentMetaTypeLowered == typeof(string).ToString().ToLower())
                {
                    metaToAdd.m_oTagMeta.m_sType = ApiObjects.MetaType.MultilingualString.ToString();
                }
                else if (currentMetaTypeLowered == typeof(double).ToString().ToLower())
                {
                    metaToAdd.m_oTagMeta.m_sType = ApiObjects.MetaType.Number.ToString();
                }
                else if (currentMetaTypeLowered == typeof(DateTime).ToString().ToLower())
                {
                    metaToAdd.m_oTagMeta.m_sType = ApiObjects.MetaType.DateTime.ToString();
                }
                else
                {
                    throw new Exception("Unknown meta type when ConvertMediaObjMetasToAssetMetas");
                }

                result.Add(metaToAdd);
            }

            return result;
        }

        /// <summary>
        /// Fill current Asset data members with given asset only if they are empty\null (without metas and tags)
        /// </summary>
        /// <param name="asset">given asset to fill with</param>
        public virtual bool UpdateFields(Asset asset)
        {
            bool needToUpdateBasicData = false;

            if (asset != null)
            {
                this.Id = asset.Id;
                this.AssetType = asset.AssetType;
                this.CreateDate = asset.CreateDate;
                this.StartDate = this.StartDate.GetUpdatedValue(asset.StartDate, ref needToUpdateBasicData);
                this.EndDate = this.EndDate.GetUpdatedValue(asset.EndDate, ref needToUpdateBasicData);
                this.Name = this.Name.GetUpdatedValue(asset.Name, ref needToUpdateBasicData);
                this.Description = this.Description.GetUpdatedValue(asset.Description, ref needToUpdateBasicData);

                if (this.Images == null || this.Images.Count == 0)
                {
                    this.Images = asset.Images;
                }

                if (this.CoGuid.IsNullOrEmptyOrWhiteSpace())
                {
                    this.CoGuid = asset.CoGuid;
                }
            }

            return needToUpdateBasicData;
        }

        #region IExcel Methods
        
        public virtual Dictionary<string, object> GetExcelValues(int groupId)
        {
            Dictionary<string, object> excelValues = new Dictionary<string, object>();
            
            if (!string.IsNullOrEmpty(this.CoGuid))
            {
                var excelColumn = ExcelColumn.GetFullColumnName(Asset.EXTERNAL_ASSET_ID, null, null, true);
                excelValues.Add(excelColumn, this.CoGuid);
            }

            if (!string.IsNullOrEmpty(this.Name))
            {
                var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.NAME_META_SYSTEM_NAME);
                excelValues.Add(excelColumn, this.Name);
            }

            if (this.NamesWithLanguages != null && this.NamesWithLanguages.Count > 0)
            {
                foreach (var nameInOtherLang in this.NamesWithLanguages)
                {
                    if (!nameInOtherLang.IsDefault)
                    {
                        var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.NAME_META_SYSTEM_NAME, nameInOtherLang.LanguageCode);
                        excelValues.Add(excelColumn, nameInOtherLang.Value);
                    }
                }
            }

            if (!string.IsNullOrEmpty(this.Description))
            {
                var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.DESCRIPTION_META_SYSTEM_NAME);
                excelValues.Add(excelColumn, this.Description);
            }

            if (this.DescriptionsWithLanguages != null && this.DescriptionsWithLanguages.Count > 0)
            {
                foreach (var descriptionInOtherLang in this.DescriptionsWithLanguages)
                {
                    if (!descriptionInOtherLang.IsDefault)
                    {
                        var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.DESCRIPTION_META_SYSTEM_NAME, descriptionInOtherLang.LanguageCode);
                        excelValues.Add(excelColumn, descriptionInOtherLang.Value);
                    }
                }
            }

            if (this.StartDate.HasValue)
            {
                var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.PLAYBACK_START_DATE_TIME_META_SYSTEM_NAME, null, null, true);
                excelValues.TryAdd(excelColumn, this.StartDate);
            }

            if (this.EndDate.HasValue)
            {
                var excelColumn = ExcelColumn.GetFullColumnName(AssetManager.PLAYBACK_END_DATE_TIME_META_SYSTEM_NAME, null, null, true);
                excelValues.TryAdd(excelColumn, this.EndDate);
            }
            
            foreach (var meta in this.Metas)
            {
                if (!string.IsNullOrEmpty(meta.m_sValue))
                {
                    var metaColumnName = ExcelColumn.GetFullColumnName(meta.m_oTagMeta.m_sName);
                    excelValues.TryAdd(metaColumnName, meta.m_sValue);
                }

                if (meta.Value != null)
                {
                    foreach (var languageContainer in meta.Value)
                    {
                        string language = null;
                        if (!languageContainer.IsDefault)
                        {
                            language = languageContainer.LanguageCode;
                        }
                        var metaColumnName = ExcelColumn.GetFullColumnName(meta.m_oTagMeta.m_sName, language);
                        if (!string.IsNullOrEmpty(languageContainer.Value))
                        {
                            excelValues.TryAdd(metaColumnName, languageContainer.Value);
                        }
                    }
                }
            }
            
            foreach (var tag in Tags)
            {
                var tagColumnName = ExcelColumn.GetFullColumnName(tag.m_oTagMeta.m_sName);
                var tagValues = tag.m_lValues != null && tag.m_lValues.Count > 0 ? string.Join(",", tag.m_lValues) : null;
                if (tagValues != null)
                {
                    excelValues.TryAdd(tagColumnName, tagValues);
                }
            }
            
            return excelValues;
        }

        public virtual void SetExcelValues(int groupId, Dictionary<string, object> columnNamesToValues, Dictionary<string, ExcelColumn> columns, IExcelStructure structureObject)
        {
        }

        protected void SetMetaByExcelValues(KeyValuePair<string, object> columnValue, ExcelColumn excelColumn, string defaultLanguage, ref Dictionary<string, List<LanguageContainer>> dicMetas)
        {
            bool isDefaultLanguage = false;
            if (string.IsNullOrEmpty(excelColumn.Language) || excelColumn.Language.Equals(defaultLanguage))
            {
                excelColumn.Language = defaultLanguage;
                isDefaultLanguage = true;
            }

            switch (excelColumn.SystemName)
            {
                case AssetManager.NAME_META_SYSTEM_NAME:
                    if (isDefaultLanguage)
                    {
                        this.Name = columnValue.Value as string;
                    }
                    else
                    {
                        this.NamesWithLanguages.Add(new LanguageContainer(excelColumn.Language, columnValue.Value as string));
                    }
                    break;
                case AssetManager.DESCRIPTION_META_SYSTEM_NAME:
                    if (isDefaultLanguage)
                    {
                        this.Description = columnValue.Value as string;
                    }
                    else
                    {
                        this.DescriptionsWithLanguages.Add(new LanguageContainer(excelColumn.Language, columnValue.Value as string));
                    }
                    break;
                default:
                    if (dicMetas.ContainsKey(excelColumn.SystemName))
                    {
                        dicMetas[excelColumn.SystemName].Add(new LanguageContainer(excelColumn.Language, columnValue.Value.ToString(), isDefaultLanguage));
                    }
                    else
                    {
                        dicMetas.Add(excelColumn.SystemName, new List<LanguageContainer>() { new LanguageContainer(excelColumn.Language, columnValue.Value.ToString(), isDefaultLanguage) });
                    }
                    break;
            }
        }

        protected void SetTagByExcelValues(KeyValuePair<string, object> columnValue, string systemName, Dictionary<string, Topic> topicsMapBySystemName, string defaultLanguage)
        {
            var tagMeta = new TagMeta();
            if (topicsMapBySystemName.ContainsKey(systemName))
            {
                tagMeta.m_sName = systemName;
                tagMeta.m_sType = topicsMapBySystemName[systemName].Type.ToString();
            }
            var values = columnValue.Value.ToString().GetItemsIn<List<string>, string>();
            List<LanguageContainer[]> languageContainers = new List<LanguageContainer[]>();
            foreach (var value in values)
            {
                LanguageContainer defaultLanguageContainer = new LanguageContainer(defaultLanguage, value, true);
                languageContainers.Add(new LanguageContainer[] { defaultLanguageContainer });
            }

            this.Tags.Add(new Tags(tagMeta, values, languageContainers));
        }

        protected void SetImageByExcelValues(KeyValuePair<string, object> columnValue, ExcelColumn excelColumn, Dictionary<string, ImageType> imageTypesMapBySystemName)
        {
            if (imageTypesMapBySystemName.ContainsKey(excelColumn.SystemName))
            {
                Image image = new Image()
                {
                    Url = columnValue.Value.ToString(),
                    ImageTypeId = imageTypesMapBySystemName[excelColumn.SystemName].Id
                };
                Images.Add(image);
            }
        }

        public virtual BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, BulkUploadResultStatus status, int index, Status errorStatus)
        {
            return null;
        }

        public virtual bool EnqueueBulkUploadResult(BulkUpload bulkUpload, int resultIndex)
        {
            return false;
        }

        #endregion
    }
}