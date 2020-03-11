using System.Collections.Generic;
using System.Linq;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;

namespace TVPApiModule.CatalogLoaders
{
    public static class Util
    {
        public static void UpdateProgramsTags(List<ProgramObj> programs, eAssetTypes assetType, List<UnifiedSearchResult> searchResults)
        {
            if (programs == null || programs.Count == 0 || searchResults == null || searchResults.Count == 0 || !(searchResults[0] is RecommendationSearchResult))
            {
                return;
            }

            var recommendationSearchResults = searchResults.Where(x => x.AssetType == assetType && x is RecommendationSearchResult)
                                                           .Select(x => x as RecommendationSearchResult)
                                                           .Where(x => x.TagsExtraData != null && x.TagsExtraData.Count > 0)
                                                           .ToDictionary(x => x.AssetId);

            if (recommendationSearchResults.Count > 0)
            {
                for (int i = 0; i < programs.Count; i++)
                {
                    if (recommendationSearchResults.ContainsKey(programs[i].AssetId))
                    {
                        var updatedProgram = programs[i].Clone();
                        bool epgTagsExists = true;
                        if (updatedProgram.m_oProgram.EPG_TAGS == null)
                        {
                            updatedProgram.m_oProgram.EPG_TAGS = new List<EPGDictionary>();
                            epgTagsExists = false;
                        }

                        foreach (var extraData in recommendationSearchResults[programs[i].AssetId].TagsExtraData)
                        {
                            if (!epgTagsExists || !updatedProgram.m_oProgram.EPG_TAGS.Any(x => x.Key == extraData.Key))
                            {
                                updatedProgram.m_oProgram.EPG_TAGS.Add(new EPGDictionary() { Key = extraData.Key, Value = extraData.Value });
                            }
                        }

                        programs[i] = updatedProgram;
                    }
                }
            }
        }

        public static T Clone<T>(this T source)
        {
            return Force.DeepCloner.DeepClonerExtensions.DeepClone<T>(source);
        }
    }
}
