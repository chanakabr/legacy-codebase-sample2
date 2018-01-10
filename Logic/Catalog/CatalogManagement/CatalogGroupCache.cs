using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class CatalogGroupCache
    {
        public LanguageObj DefaultLanguage { get; set; }
        public Dictionary<string, LanguageObj> LanguageMapByCode { get; set; }
        public Dictionary<int, LanguageObj> LanguageMapById { get; set; }        
        public Dictionary<string, AssetStruct> AssetStructsMapBySystemName { get; set; }
        public Dictionary<long, AssetStruct> AssetStructsMapById { get; set; }
        public Dictionary<string, Topic> TopicsMapBySystemName { get; set; }
        public Dictionary<long, Topic> TopicsMapById { get; set; }

        public CatalogGroupCache()
        {
            DefaultLanguage = null;
            LanguageMapByCode = new Dictionary<string, LanguageObj>(StringComparer.OrdinalIgnoreCase);
            LanguageMapById = new Dictionary<int, LanguageObj>();            
            AssetStructsMapBySystemName = new Dictionary<string, AssetStruct>(StringComparer.OrdinalIgnoreCase);
            AssetStructsMapById = new Dictionary<long, AssetStruct>();
            TopicsMapBySystemName = new Dictionary<string, Topic>(StringComparer.OrdinalIgnoreCase);
            TopicsMapById = new Dictionary<long, Topic>();            
        }

        // TODO - Lior, move all language related properties in this class to seperate cache or invalidate catalogGroupCache when adding\updating languages (doesn't exist at the moment)
        public CatalogGroupCache(List<LanguageObj> languages, List<AssetStruct> assetStructs, List<Topic> topics)
        {
            LanguageObj defaultLanguage = languages.Where(x => x.IsDefault).FirstOrDefault();
            if (defaultLanguage != null && defaultLanguage.ID > 0)
            {
                DefaultLanguage = new LanguageObj(defaultLanguage.ID, defaultLanguage.Name, defaultLanguage.Code, defaultLanguage.Direction, defaultLanguage.IsDefault, defaultLanguage.DisplayName);
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
                    }

                    if (!LanguageMapById.ContainsKey(langauge.ID))
                    {
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
                    if (!AssetStructsMapBySystemName.ContainsKey(assetStruct.SystemName))
                    {
                        AssetStructsMapBySystemName.Add(assetStruct.SystemName, assetStruct);
                    }

                    if (!AssetStructsMapById.ContainsKey(assetStruct.Id))
                    {
                        AssetStructsMapById.Add(assetStruct.Id, assetStruct);
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
                    }

                    if (!TopicsMapById.ContainsKey(topic.Id))
                    {
                        TopicsMapById.Add(topic.Id, topic);
                    }
                }
            }
        }

        public bool IsValid()
        {
            return DefaultLanguage != null && DefaultLanguage.ID > 0 && LanguageMapByCode != null && LanguageMapByCode.Count > 0 && LanguageMapById != null && LanguageMapById.Count > 0
                    && AssetStructsMapById != null && AssetStructsMapById.Count > 0 && AssetStructsMapBySystemName != null && AssetStructsMapBySystemName.Count == AssetStructsMapById.Count
                    && TopicsMapById != null && TopicsMapById.Count > 0 && TopicsMapBySystemName != null && TopicsMapBySystemName.Count == TopicsMapById.Count;
        }
    }
}
