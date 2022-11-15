using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.Catalog;
using Tvinci.Core.DAL;

namespace Core.Catalog
{
    public class CatalogGroupCache
    {
        private LanguageObj defaultLanguage;
        public LanguageObj DefaultLanguage
        {
            get
            {
                if (defaultLanguage == null || defaultLanguage.ID == 0)
                {
                    var defaultLanguageObj = LanguageMapById != null && LanguageMapById.Count > 0 ? LanguageMapById.Values.FirstOrDefault(x => x.IsDefault) : null;
                    if (defaultLanguageObj != null && defaultLanguageObj.ID > 0)
                    {
                        defaultLanguage = new LanguageObj(defaultLanguageObj.ID, defaultLanguageObj.Name, defaultLanguageObj.Code, defaultLanguageObj.Direction, defaultLanguageObj.IsDefault, defaultLanguageObj.DisplayName);
                    }
                }

                return defaultLanguage;
            }
        }

        public Dictionary<string, LanguageObj> LanguageMapByCode { get; set; }
        public Dictionary<int, LanguageObj> LanguageMapById { get; set; }
        public Dictionary<string, AssetStruct> AssetStructsMapBySystemName { get; set; }
        public Dictionary<long, AssetStruct> AssetStructsMapById { get; private set; }
        public Dictionary<string, Dictionary<string, Topic>> TopicsMapBySystemNameAndByType { get; set; }
        public Dictionary<long, Topic> TopicsMapById { get; set; }


        /// <summary>
        /// Indicates if this group has DTT regionalisation support or not
        /// </summary>
        public bool IsRegionalizationEnabled { get; set; }

        /// <summary>
        /// The default region of this group (in case a domain isn't associated with any region)
        /// </summary>
        public int DefaultRegion { get; set; }

        public int DefaultRecommendationEngine { get; set; }

        /// <summary>
        /// The group's default recommendation engine
        /// </summary>
        public int RelatedRecommendationEngine { get; set; }

        /// <summary>
        /// The group's default recommendation engine
        /// </summary>
        public int SearchRecommendationEngine { get; set; }

        /// <summary>
        /// The group's default recommendation engine
        /// </summary>
        public int RelatedRecommendationEngineEnrichments { get; set; }

        /// <summary>
        /// The group's default recommendation engine
        /// </summary>
        public int SearchRecommendationEngineEnrichments { get; set; }

        public bool IsGeoAvailabilityWindowingEnabled { get; set; }

        public bool IsAssetUserRuleEnabled { get; set; }

        private long programAssetStructId;

        public CatalogGroupCache()
        {
            defaultLanguage = null;
            LanguageMapByCode = new Dictionary<string, LanguageObj>(StringComparer.OrdinalIgnoreCase);
            LanguageMapById = new Dictionary<int, LanguageObj>();
            AssetStructsMapBySystemName = new Dictionary<string, AssetStruct>(StringComparer.OrdinalIgnoreCase);
            AssetStructsMapById = new Dictionary<long, AssetStruct>();
            TopicsMapBySystemNameAndByType = new Dictionary<string, Dictionary<string, Topic>>(StringComparer.OrdinalIgnoreCase);
            TopicsMapById = new Dictionary<long, Topic>();
            programAssetStructId = 0;
        }

        // TODO Lior - move all language related properties in this class to seperate cache or invalidate catalogGroupCache when adding\updating languages (doesn't exist at the moment)
        public CatalogGroupCache(int groupId, List<LanguageObj> languages, List<AssetStruct> assetStructs, List<Topic> topics)
        {
            LanguageObj defaultLanguageObj = languages.FirstOrDefault(x => x.IsDefault);
            if (defaultLanguageObj != null && defaultLanguageObj.ID > 0)
            {
                defaultLanguage = new LanguageObj(defaultLanguageObj.ID, defaultLanguageObj.Name, defaultLanguageObj.Code, defaultLanguageObj.Direction, defaultLanguageObj.IsDefault, defaultLanguageObj.DisplayName);
            }

            LanguageMapByCode = new Dictionary<string, LanguageObj>(StringComparer.OrdinalIgnoreCase);
            LanguageMapById = new Dictionary<int, LanguageObj>();
            if (languages != null && languages.Count > 0)
            {
                foreach (LanguageObj langauge in languages)
                {
                    if (!LanguageMapByCode.ContainsKey(langauge.Code) && !LanguageMapById.ContainsKey(langauge.ID))
                    {
                        LanguageMapByCode.Add(langauge.Code, langauge);
                        LanguageMapById.Add(langauge.ID, langauge);
                    }
                }
            }

            AssetStructsMapBySystemName = new Dictionary<string, AssetStruct>(StringComparer.OrdinalIgnoreCase);
            AssetStructsMapById = new Dictionary<long, AssetStruct>();
            if (assetStructs != null && assetStructs.Count > 0)
            {
                foreach (AssetStruct assetStruct in assetStructs)
                {
                    if (!AssetStructsMapBySystemName.ContainsKey(assetStruct.SystemName) && !AssetStructsMapById.ContainsKey(assetStruct.Id))
                    {
                        AssetStructsMapBySystemName.Add(assetStruct.SystemName, assetStruct);
                        AssetStructsMapById.Add(assetStruct.Id, assetStruct);
                    }
                }
            }

            TopicsMapBySystemNameAndByType = new Dictionary<string, Dictionary<string, Topic>>(StringComparer.OrdinalIgnoreCase);
            TopicsMapById = new Dictionary<long, Topic>();
            if (topics != null && topics.Count > 0)
            {
                foreach (Topic topic in topics)
                {
                    if (!TopicsMapBySystemNameAndByType.ContainsKey(topic.SystemName))
                    {
                        TopicsMapBySystemNameAndByType.Add(topic.SystemName, new Dictionary<string, Topic>((StringComparer.OrdinalIgnoreCase)));
                    }

                    if (!TopicsMapById.ContainsKey(topic.Id) && !TopicsMapBySystemNameAndByType[topic.SystemName].ContainsKey(topic.Type.ToString()))
                    {
                        TopicsMapById.Add(topic.Id, topic);
                        TopicsMapBySystemNameAndByType[topic.SystemName].Add(topic.Type.ToString(), topic);
                    }
                }
            }

            SetCatalogGroupCacheDefaults(groupId, this);
            SetProgramAssetStructId();
        }

        public bool IsValid()
        {
            var DefaultLanguage = GetDefaultLanguage();
            return DefaultLanguage != null && DefaultLanguage.ID > 0 && LanguageMapByCode != null && LanguageMapByCode.Count > 0 && LanguageMapById != null && LanguageMapById.Count > 0
                    && AssetStructsMapById != null && AssetStructsMapById.Count > 0 && AssetStructsMapBySystemName != null && AssetStructsMapBySystemName.Count == AssetStructsMapById.Count
                    && TopicsMapById != null && TopicsMapById.Count > 0 && TopicsMapBySystemNameAndByType != null;
        }

        private static void SetCatalogGroupCacheDefaults(int groupId, CatalogGroupCache catalogGroupCache)
        {
            CatalogDAL.GetGroupDefaultParameters(groupId,
                out var isRegionalizationEnabled,
                out var defaultRegion,
                out var defaultRecommendationEngine,
                out var relatedRecommendationEngine,
                out var searchRecommendationEngine,
                out var relatedRecommendationEngineEnrichments,
                out var searchRecommendationEngineEnrichments,
                out var isGeoAvailabilityEnabled,
                out var isAssetUserRuleEnabled);
            catalogGroupCache.IsRegionalizationEnabled = isRegionalizationEnabled;
            catalogGroupCache.DefaultRegion = defaultRegion;
            catalogGroupCache.DefaultRecommendationEngine = defaultRecommendationEngine;
            catalogGroupCache.RelatedRecommendationEngine = relatedRecommendationEngine;
            catalogGroupCache.SearchRecommendationEngine = searchRecommendationEngine;
            catalogGroupCache.RelatedRecommendationEngineEnrichments = relatedRecommendationEngineEnrichments;
            catalogGroupCache.SearchRecommendationEngineEnrichments = searchRecommendationEngineEnrichments;
            catalogGroupCache.IsGeoAvailabilityWindowingEnabled = isGeoAvailabilityEnabled;
            catalogGroupCache.IsAssetUserRuleEnabled = isAssetUserRuleEnabled;
        }

        private void SetProgramAssetStructId()
        {
            var programAssetStruct = AssetStructsMapById.Values.FirstOrDefault(x => x.IsProgramAssetStruct);
            if (programAssetStruct != null)
            {
                programAssetStructId = programAssetStruct.Id;
            }
        }

        public long GetRealAssetStructId(long assetStructId, out bool isProgramAssetStruct)
        {
            var realAssetStructId = assetStructId;
            isProgramAssetStruct = false;

            if (GetProgramAssetStructId() != 0 && (assetStructId == 0 || assetStructId == GetProgramAssetStructId()))
            {
                realAssetStructId = GetProgramAssetStructId();
                isProgramAssetStruct = true;
            }

            return realAssetStructId;
        }

        public HashSet<long> GetObjectVirtualAssetIds()
        {
            HashSet<long> ObjectVirtualAssetIds = null;
            var ids = AssetStructsMapById.Values.Where(x => x.IsObjectVirtualAsset).Select(x => x.Id).ToList();
            if (ids?.Count > 0)
            {
                ObjectVirtualAssetIds = new HashSet<long>(ids);
            }

            return ObjectVirtualAssetIds;
        }

        public LanguageObj GetDefaultLanguage()
        {
            if (defaultLanguage == null || defaultLanguage.ID == 0)
            {
                var defaultLanguageObj = LanguageMapById != null && LanguageMapById.Count > 0 ? LanguageMapById.Values.FirstOrDefault(x => x.IsDefault) : null;
                if (defaultLanguageObj != null && defaultLanguageObj.ID > 0)
                {
                    defaultLanguage = new LanguageObj(defaultLanguageObj.ID, defaultLanguageObj.Name, defaultLanguageObj.Code, defaultLanguageObj.Direction, defaultLanguageObj.IsDefault, defaultLanguageObj.DisplayName);
                }
            }

            return defaultLanguage;
        }

        public long GetProgramAssetStructId()
        {
            if (programAssetStructId == 0)
            {
                SetProgramAssetStructId();
            }

            return programAssetStructId;
        }

        public AssetStructMeta GetAssetStructMetaBySystemName(int groupId, long assetStructId, string systemName)
        {
            if (TopicsMapBySystemNameAndByType.ContainsKey(systemName))
            {
                var t = TopicsMapBySystemNameAndByType[systemName].FirstOrDefault(key => key.Key != MetaType.Tag.ToString());
                if (!IsDefault(t))
                {
                    if (AssetStructsMapById.ContainsKey(assetStructId))
                    {
                        var assetStruct = this.AssetStructsMapById[assetStructId];
                        if (assetStruct.AssetStructMetas.ContainsKey(t.Value.Id))
                        {
                            return assetStruct.AssetStructMetas[t.Value.Id];
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Check if struct object is default (null)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsDefault<T>(T value) where T : struct
        {
            bool isDefault = value.Equals(default(T));

            return isDefault;
        }
    }
}
