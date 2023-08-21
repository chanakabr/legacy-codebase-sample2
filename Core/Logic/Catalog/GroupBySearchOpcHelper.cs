using System;
using ApiObjects.SearchObjects;
using Core.Catalog;

namespace ApiLogic.Catalog
{
    public static class GroupBySearchOpcHelper
    {
        private const string ReverseMetaTagSearchOrderForGroupByEnvName = "REVERSE_META_TAG_SEARCH_ORDER_FOR_GROUP_BY";
        private static readonly bool ReverseMetaTagSearchOrderForGroupBy = bool.Parse(Environment.GetEnvironmentVariable(ReverseMetaTagSearchOrderForGroupByEnvName) ?? false.ToString());

        public static eFieldType ExtractDefinitionType(string lowered, CatalogGroupCache catalogGroupCache, UnifiedSearchDefinitions definitions)
        {
            // IMPORTANT!
            // This specific if there for VF (as of time writing that) only.
            // Due to OPC Migration few metas (aka Series Id, Episode Number, etc.) are duplicated but have different types (Meta + Tag)
            // It leads to confusion while building the Group By ElasticSearch Query, so we've changed the order to check for meta at first glance.
            if (ReverseMetaTagSearchOrderForGroupBy)
            {
                return CheckForMeta() ?? CheckForTag() ?? eFieldType.NonStringMeta;
            }

            // Original functionality for ordering!
            return CheckForTag() ?? CheckForMeta() ?? eFieldType.NonStringMeta;

            eFieldType? CheckForMeta()
            {
                if (definitions.shouldSearchEpg ||
                    definitions.shouldSearchRecordings ||
                    catalogGroupCache.TopicsMapBySystemNameAndByType[lowered].ContainsKey(ApiObjects.MetaType.String.ToString()) ||
                    catalogGroupCache.TopicsMapBySystemNameAndByType[lowered].ContainsKey(ApiObjects.MetaType.MultilingualString.ToString()) ||
                    catalogGroupCache.TopicsMapBySystemNameAndByType[lowered].ContainsKey(ApiObjects.MetaType.ReleatedEntity.ToString()))
                {
                    return eFieldType.StringMeta;
                }

                return null;
            }

            eFieldType? CheckForTag()
            {
                if (catalogGroupCache.TopicsMapBySystemNameAndByType[lowered].ContainsKey(ApiObjects.MetaType.Tag.ToString()))
                {
                    return eFieldType.Tag;
                }

                return null;
            }
        }
    }
}
