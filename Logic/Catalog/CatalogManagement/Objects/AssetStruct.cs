using ApiObjects;
using ApiObjects.Response;
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
        public string Name { get; set; }
        public List<LanguageContainer> NamesInOtherLanguages { get; set; }
        public string SystemName { get; set; }
        public List<long> MetaIds { get; set; }
        public bool? IsPredefined { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }
        // TODO - Need to support adding/editing AssociationTag per asset struct
        public string AssociationTag { get; set; }
        /// <summary>
        /// Asset Struct Meta list (the key is the metaID)
        /// </summary>
        public Dictionary<long, AssetStructMeta> AssetStructMetas { get; set; }

        public HashSet<string> Features { get; set; }
        public long? ParentId { get; set; }
        public long? ConnectingMetaId { get; set; }
        public long? ConnectedParentMetaId { get; set; }
        public string PluralName { get; set; }

        public AssetStruct()
        {
            this.Id = 0;
            this.Name = string.Empty;
            this.NamesInOtherLanguages = new List<LanguageContainer>();
            this.SystemName = string.Empty;
            this.MetaIds = new List<long>();
            this.IsPredefined = null;
            this.AssociationTag = string.Empty;
            this.ParentId = 0;
            this.CreateDate = 0;
            this.UpdateDate = 0;
            this.AssetStructMetas = new Dictionary<long, AssetStructMeta>();
            this.Features = new HashSet<string>();
            this.ConnectingMetaId = 0;
            this.ConnectedParentMetaId = 0;
            this.PluralName = string.Empty;
        }

        public AssetStruct(long id, string name, List<LanguageContainer> namesInOtherLanguages, string systemName, bool isPredefined, string associationTag, 
                           long parentId, long createDate, long updateDate, HashSet<string> features, long connectingMetaId, long connectedParentMetaId, string pluralName)
        {
            this.Id = id;
            this.Name = name;
            this.NamesInOtherLanguages = new List<LanguageContainer>(namesInOtherLanguages);
            this.SystemName = systemName;
            this.MetaIds = new List<long>();
            this.IsPredefined = isPredefined;
            this.AssociationTag = associationTag;
            this.ParentId = parentId;
            this.CreateDate = createDate;
            this.UpdateDate = updateDate;
            this.AssetStructMetas = new Dictionary<long, AssetStructMeta>();
            this.Features = features != null ? new HashSet<string>(features, StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.ConnectingMetaId = connectingMetaId;
            this.ConnectedParentMetaId = connectedParentMetaId;
            this.PluralName = pluralName;
        }

        public AssetStruct(AssetStruct assetStructToCopy)
        {
            this.Id = assetStructToCopy.Id;
            this.Name = string.Copy(assetStructToCopy.SystemName);
            this.NamesInOtherLanguages = new List<LanguageContainer>(assetStructToCopy.NamesInOtherLanguages);
            this.SystemName = string.Copy(assetStructToCopy.SystemName);
            this.MetaIds = new List<long>(assetStructToCopy.MetaIds);
            this.IsPredefined = assetStructToCopy.IsPredefined;
            this.AssociationTag = assetStructToCopy.AssociationTag;
            this.ParentId = assetStructToCopy.ParentId;
            this.CreateDate = assetStructToCopy.CreateDate;
            this.UpdateDate = assetStructToCopy.UpdateDate;
            this.AssetStructMetas = new Dictionary<long, AssetStructMeta>(assetStructToCopy.AssetStructMetas);
            this.Features = new HashSet<string>(assetStructToCopy.Features, StringComparer.OrdinalIgnoreCase);
            this.ConnectingMetaId = assetStructToCopy.ConnectingMetaId;
            this.ConnectedParentMetaId = assetStructToCopy.ConnectedParentMetaId;
            this.PluralName = assetStructToCopy.PluralName;
        }

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

        public Status ValidateBasicMetaIds(CatalogGroupCache catalogGroupCache)
        {
            Status result = new Status((int)eResponseStatus.AssetStructMissingBasicMetaIds, eResponseStatus.AssetStructMissingBasicMetaIds.ToString());
            List<long> basicMetaIds = new List<long>();
            if (catalogGroupCache.TopicsMapBySystemName != null && catalogGroupCache.TopicsMapBySystemName.Count > 0)
            {
                basicMetaIds = catalogGroupCache.TopicsMapBySystemName.Where(x => AssetManager.BasicMetasSystemNames.Contains(x.Key.ToLower())).Select(x => x.Value.Id).ToList();
                if (this.MetaIds != null)
                {
                    List<long> noneExistingBasicMetaIds = basicMetaIds.Except(this.MetaIds).ToList();
                    if (noneExistingBasicMetaIds == null || noneExistingBasicMetaIds.Count == 0)
                    {
                        result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                    else
                    {
                        result = new Status((int)eResponseStatus.AssetStructMissingBasicMetaIds, string.Format("{0} for the following Meta Ids: {1}",
                                            eResponseStatus.AssetStructMissingBasicMetaIds.ToString(), string.Join(",", noneExistingBasicMetaIds)));
                    }
                }
            }

            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("Id: {0}, ", Id));
            sb.AppendFormat("Name: {0},", Name); 
            sb.AppendFormat("NamesInOtherLanguages: {0}, ", NamesInOtherLanguages != null && NamesInOtherLanguages.Count > 0 ?
                                                            string.Join(",", NamesInOtherLanguages.Select(x => string.Format("languageCode: {0}, value: {1}", x.LanguageCode, x.Value)).ToList()) : string.Empty);
            sb.AppendFormat("SystemName: {0}, ", SystemName);
            sb.AppendFormat("MetaIds: {0}, ", MetaIds != null ? string.Join(",", MetaIds) : string.Empty);
            sb.AppendFormat("IsPredefined: {0}, ", IsPredefined.HasValue ? IsPredefined.Value.ToString() : string.Empty);
            sb.AppendFormat("AssociationTag: {0}, ", AssociationTag);
            sb.AppendFormat("ParentId: {0}, ", ParentId.HasValue ? ParentId.Value.ToString() : string.Empty);
            sb.AppendFormat("CreateDate: {0}, ", CreateDate);
            sb.AppendFormat("UpdateDate: {0}", UpdateDate);
            sb.AppendFormat("AssetStructMetas: {0}, ", AssetStructMetas != null && AssetStructMetas.Count > 0 ?
                                                      string.Join(",", AssetStructMetas.Select(x => x.Value.ToString()).ToList()) : string.Empty);
            sb.AppendFormat("Features: {0}, ", (Features != null && Features.Count > 0) ? string.Join(",", Features) : string.Empty);
            sb.AppendFormat("ConnectingMetaId: {0}, ", ConnectingMetaId.HasValue ? ConnectingMetaId.Value.ToString() : string.Empty);
            sb.AppendFormat("ConnectedParentMetaId: {0}, ", ConnectedParentMetaId.HasValue ? ConnectedParentMetaId.Value.ToString() : string.Empty);
            sb.AppendFormat("PluralName: {0},", PluralName);

            return sb.ToString();
        }

    }
}
