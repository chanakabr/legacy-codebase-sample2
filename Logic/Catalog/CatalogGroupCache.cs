using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<long, AssetStruct> AssetStructsMapById { get; set; }
        public Dictionary<string, Topic> TopicsMapBySystemName { get; set; }
        public Dictionary<long, Topic> TopicsMapById { get; set; }

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
        public long ProgramAssetStructId
        {
            get
            {
                if (programAssetStructId == 0)
                { 
                    var programAssetStruct = AssetStructsMapById.Values.FirstOrDefault(x => x.IsProgramAssetStruct);
                    if (programAssetStruct != null)
                    {
                        programAssetStructId = programAssetStruct.Id;
                    }
                }

                return programAssetStructId;
            }
        }

        public CatalogGroupCache()
        {
            defaultLanguage = null;
            LanguageMapByCode = new Dictionary<string, LanguageObj>(StringComparer.OrdinalIgnoreCase);
            LanguageMapById = new Dictionary<int, LanguageObj>();            
            AssetStructsMapBySystemName = new Dictionary<string, AssetStruct>(StringComparer.OrdinalIgnoreCase);
            AssetStructsMapById = new Dictionary<long, AssetStruct>();
            TopicsMapBySystemName = new Dictionary<string, Topic>(StringComparer.OrdinalIgnoreCase);
            TopicsMapById = new Dictionary<long, Topic>();
            programAssetStructId = 0;
        }

        // TODO - Lior, move all language related properties in this class to seperate cache or invalidate catalogGroupCache when adding\updating languages (doesn't exist at the moment)
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
                    if (!LanguageMapByCode.ContainsKey(langauge.Code))
                    {
                        LanguageMapByCode.Add(langauge.Code, langauge);

                        if (!LanguageMapById.ContainsKey(langauge.ID))
                        {
                            LanguageMapById.Add(langauge.ID, langauge);
                        }
                    }
                }
            }

            AssetStructsMapBySystemName = new Dictionary<string, AssetStruct>(StringComparer.OrdinalIgnoreCase);
            AssetStructsMapById = new Dictionary<long, AssetStruct>();
            if (assetStructs != null && assetStructs.Count > 0)
            {
                foreach (AssetStruct assetStruct in assetStructs)
                {
                    if (!AssetStructsMapBySystemName.ContainsKey(assetStruct.SystemName))
                    {
                        AssetStructsMapBySystemName.Add(assetStruct.SystemName, assetStruct);

                        if (!AssetStructsMapById.ContainsKey(assetStruct.Id))
                        {
                            AssetStructsMapById.Add(assetStruct.Id, assetStruct);
                        }
                    }
                }
            }

            TopicsMapBySystemName = new Dictionary<string, Topic>(StringComparer.OrdinalIgnoreCase);
            TopicsMapById = new Dictionary<long, Topic>();
            if (topics != null && topics.Count > 0)
            {
                foreach (Topic topic in topics)
                {
                    if (!TopicsMapBySystemName.ContainsKey(topic.SystemName))
                    {
                        TopicsMapBySystemName.Add(topic.SystemName, topic);

                        if (!TopicsMapById.ContainsKey(topic.Id))
                        {
                            TopicsMapById.Add(topic.Id, topic);
                        }
                    }
                }
            }

            SetCatalogGroupCacheDefaults(groupId, this);

            var programAssetStruct = AssetStructsMapById.Values.FirstOrDefault(x => x.IsProgramAssetStruct);
            if (programAssetStruct != null)
            {
                programAssetStructId = programAssetStruct.Id;
            }
        }

        public bool IsValid()
        {
            return DefaultLanguage != null && DefaultLanguage.ID > 0 && LanguageMapByCode != null && LanguageMapByCode.Count > 0 && LanguageMapById != null && LanguageMapById.Count > 0
                    && AssetStructsMapById != null && AssetStructsMapById.Count > 0 && AssetStructsMapBySystemName != null && AssetStructsMapBySystemName.Count == AssetStructsMapById.Count
                    && TopicsMapById != null && TopicsMapById.Count > 0 && TopicsMapBySystemName != null && TopicsMapBySystemName.Count == TopicsMapById.Count;
        }

        private static void SetCatalogGroupCacheDefaults(int groupId, CatalogGroupCache catalogGroupCache)
        {
            bool isRegionalizationEnabled, isGeoAvailabilityEnabled, isAssetUserRuleEnabled;
            int defaultRegion, defaultRecommendationEngine, relatedRecommendationEngine, searchRecommendationEngine, relatedRecommendationEngineEnrichments, searchRecommendationEngineEnrichments;      
            
            CatalogDAL.GetGroupDefaultParameters(groupId, out isRegionalizationEnabled, out defaultRegion, out defaultRecommendationEngine, out relatedRecommendationEngine, out searchRecommendationEngine,
                                                out relatedRecommendationEngineEnrichments, out searchRecommendationEngineEnrichments, out isGeoAvailabilityEnabled, out isAssetUserRuleEnabled);
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
    }
}