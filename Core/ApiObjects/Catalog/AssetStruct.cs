using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.Catalog
{
    public class AssetStruct
    {
        private const string OBJECT_VIRTUAL_ASSET = "ObjectVirtualAsset";

        #region Data Members

        public long Id { get; set; }
        public string Name { get; set; }
        public List<LanguageContainer> NamesInOtherLanguages { get; set; }
        public string SystemName { get; set; }
        public List<long> MetaIds { get; set; }
        public bool? IsPredefined { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }

        /// <summary>
        /// Asset Struct Meta list (the key is the metaID)
        /// </summary>
        public Dictionary<long, AssetStructMeta> AssetStructMetas { get; set; }

        public HashSet<string> Features { get; set; }
        public long? ParentId { get; set; }
        public long? ConnectingMetaId { get; set; }
        public long? ConnectedParentMetaId { get; set; }
        public List<KeyValuePair<string, string>> DynamicData;
        public string PluralName { get; set; }

        // currently used only for internal use for migration and migrated accounts
        public bool IsLinearAssetStruct { get; set; }

        // currently used only for internal use for migration and migrated accounts
        public bool IsProgramAssetStruct { get; set; }

        [JsonIgnore()] public Dictionary<string, Topic> TopicsMapBySystemName;

        public bool IsSeriesAssetStruct
        {
            get { return this.SystemName.ToLower() == "series"; }
        }

        public bool IsObjectVirtualAsset
        {
            get { return Features != null && Features.Contains(OBJECT_VIRTUAL_ASSET); }
        }

        #endregion

        #region Ctor's

        public AssetStruct()
        {
            this.Id = 0;
            this.Name = string.Empty;
            this.NamesInOtherLanguages = new List<LanguageContainer>();
            this.SystemName = string.Empty;
            this.MetaIds = new List<long>();
            this.IsPredefined = null;
            this.ParentId = null;
            this.CreateDate = 0;
            this.UpdateDate = 0;
            this.AssetStructMetas = new Dictionary<long, AssetStructMeta>();
            this.Features = new HashSet<string>();
            this.ConnectingMetaId = 0;
            this.ConnectedParentMetaId = 0;
            this.PluralName = string.Empty;
            this.DynamicData = null;
        }

        public AssetStruct(AssetStruct assetStructToCopy)
        {
            Copy(assetStructToCopy);
        }

        #endregion

        public string GetCommaSeparatedFeatures()
        {
            if (this.Features != null && this.Features.Count > 0)
            {
                return string.Join(",", this.Features);
            }
            else
            {
                return string.Empty;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("Id: {0}, ", Id));
            sb.AppendFormat("Name: {0},", Name);
            sb.AppendFormat("NamesInOtherLanguages: {0}, ", NamesInOtherLanguages != null && NamesInOtherLanguages.Count > 0 ? string.Join(",", NamesInOtherLanguages.Select(x => string.Format("languageCode: {0}, value: {1}", x.m_sLanguageCode3, x.m_sValue)).ToList()) : string.Empty);
            sb.AppendFormat("SystemName: {0}, ", SystemName);
            sb.AppendFormat("MetaIds: {0}, ", MetaIds != null ? string.Join(",", MetaIds) : string.Empty);
            sb.AppendFormat("IsPredefined: {0}, ", IsPredefined.HasValue ? IsPredefined.Value.ToString() : string.Empty);
            sb.AppendFormat("ParentId: {0}, ", ParentId.HasValue ? ParentId.Value.ToString() : string.Empty);
            sb.AppendFormat("CreateDate: {0}, ", CreateDate);
            sb.AppendFormat("UpdateDate: {0}", UpdateDate);
            sb.AppendFormat("AssetStructMetas: {0}, ", AssetStructMetas != null && AssetStructMetas.Count > 0 ? string.Join(",", AssetStructMetas.Select(x => x.Value.ToString()).ToList()) : string.Empty);
            sb.AppendFormat("Features: {0}, ", (Features != null && Features.Count > 0) ? string.Join(",", Features) : string.Empty);
            sb.AppendFormat("ConnectingMetaId: {0}, ", ConnectingMetaId.HasValue ? ConnectingMetaId.Value.ToString() : string.Empty);
            sb.AppendFormat("ConnectedParentMetaId: {0}, ", ConnectedParentMetaId.HasValue ? ConnectedParentMetaId.Value.ToString() : string.Empty);
            sb.AppendFormat("PluralName: {0},", PluralName);
            sb.AppendFormat("IsProgramAssetStruct: {0},", IsProgramAssetStruct);
            sb.AppendFormat("IsLinearAssetStruct: {0},", IsLinearAssetStruct);

            return sb.ToString();
        }

        public void Copy(AssetStruct assetStructToCopy)
        {
            this.Id = assetStructToCopy.Id;
            this.NamesInOtherLanguages = new List<LanguageContainer>(assetStructToCopy.NamesInOtherLanguages);
            var defaultName = this.NamesInOtherLanguages?.FirstOrDefault(x => x.IsDefault)?.m_sValue;
            this.Name = string.IsNullOrEmpty(assetStructToCopy.Name) ? defaultName : assetStructToCopy.Name;
            this.SystemName = assetStructToCopy.SystemName;
            this.MetaIds = new List<long>(assetStructToCopy.MetaIds);
            this.IsPredefined = assetStructToCopy.IsPredefined;
            this.ParentId = assetStructToCopy.ParentId;
            this.CreateDate = assetStructToCopy.CreateDate;
            this.UpdateDate = assetStructToCopy.UpdateDate;
            this.AssetStructMetas = new Dictionary<long, AssetStructMeta>(assetStructToCopy.AssetStructMetas);
            this.Features = assetStructToCopy.Features != null ? new HashSet<string>(assetStructToCopy.Features, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.ConnectingMetaId = assetStructToCopy.ConnectingMetaId;
            this.ConnectedParentMetaId = assetStructToCopy.ConnectedParentMetaId;
            this.PluralName = assetStructToCopy.PluralName;
            this.IsProgramAssetStruct = assetStructToCopy.IsProgramAssetStruct;
            this.IsLinearAssetStruct = assetStructToCopy.IsLinearAssetStruct;
            this.DynamicData = assetStructToCopy.DynamicData;
        }
    }
}