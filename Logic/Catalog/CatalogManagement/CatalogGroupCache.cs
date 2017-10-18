using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class CatalogGroupCache
    {

        public Dictionary<string, AssetStruct> AssetStructsMapBySystemName { get; set; }
        public Dictionary<long, AssetStruct> AssetStructsMapById { get; set; }
        public Dictionary<string, Topic> TopicsMapBySystemName { get; set; }
        public Dictionary<long, Topic> TopicsMapById { get; set; }

        public CatalogGroupCache()
        {
            AssetStructsMapBySystemName = new Dictionary<string, AssetStruct>();
            AssetStructsMapById = new Dictionary<long, AssetStruct>();
            TopicsMapBySystemName = new Dictionary<string, Topic>();
            TopicsMapById = new Dictionary<long, Topic>();
        }

        public CatalogGroupCache(List<AssetStruct> assetStructs, List<Topic> topics)
        {            
            AssetStructsMapBySystemName = new Dictionary<string, AssetStruct>();
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

            TopicsMapBySystemName = new Dictionary<string, Topic>();
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
            return AssetStructsMapById != null && AssetStructsMapById.Count > 0
                && AssetStructsMapBySystemName != null && AssetStructsMapBySystemName.Count == AssetStructsMapById.Count
                && TopicsMapById != null && TopicsMapById.Count > 0
                && TopicsMapBySystemName != null && TopicsMapBySystemName.Count == TopicsMapById.Count;
        }
    }
}
