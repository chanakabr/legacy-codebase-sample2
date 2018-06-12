using System.Collections.Generic;
using System.Linq;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;

namespace TVPApiModule.CatalogLoaders
{
    public static class Util
    {
        public static void UpdateEPGTags(List<ProgramObj> epgs, List<UnifiedSearchResult> searchResults)
        {
            if (epgs == null || searchResults == null || searchResults.Count == 0 || !(searchResults[0] is RecommendationSearchResult))
            {
                return;
            }

            UnifiedSearchResult unifiedSearchResult = null;
            RecommendationSearchResult recommendationSearchResult = null;

            foreach (var programObj in epgs)
            {
                unifiedSearchResult = searchResults.Where(x => x.AssetId == programObj.AssetId).SingleOrDefault();

                recommendationSearchResult = unifiedSearchResult as RecommendationSearchResult;
                if (recommendationSearchResult != null && recommendationSearchResult.TagsExtraData != null)
                {
                    if (programObj.m_oProgram.EPG_TAGS == null)
                    {
                        programObj.m_oProgram.EPG_TAGS = new List<EPGDictionary>();
                    }

                    foreach (var extraData in recommendationSearchResult.TagsExtraData)
                    {
                        var isKeyExist = programObj.m_oProgram.EPG_TAGS.Where(x => x.Key == extraData.Key).SingleOrDefault();
                        if (string.IsNullOrEmpty(isKeyExist.Key))
                        {
                            programObj.m_oProgram.EPG_TAGS.Add(new EPGDictionary() { Key = extraData.Key, Value = extraData.Value });
                        }
                    }
                }
            }
        }
    }
}
