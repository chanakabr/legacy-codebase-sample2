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

        public static void UpdateEPGAndRecordingTags(List<ProgramObj> epgs, List<ProgramObj> recordings, List<UnifiedSearchResult> searchResults)
        {
            if (epgs == null || searchResults == null || searchResults.Count == 0 || !(searchResults[0] is RecommendationSearchResult))
            {
                return;
            }

            HashSet<string> duplicateIds = new HashSet<string>();
            if (epgs != null && epgs.Count > 0 && recordings != null && recordings.Count > 0)
            {
                duplicateIds = new HashSet<string>(epgs.Select(x => x.AssetId).Intersect(recordings.Select(x => x.AssetId)));
            }

            foreach (UnifiedSearchResult res in searchResults)
            {
                RecommendationSearchResult recommendationSearchResult = res as RecommendationSearchResult;
                if (recommendationSearchResult != null && recommendationSearchResult.TagsExtraData != null &&
                    ((recommendationSearchResult.AssetType == eAssetTypes.EPG && epgs.Any(x => x.AssetId == res.AssetId))
                    || (recommendationSearchResult.AssetType == eAssetTypes.NPVR && recordings.Any(x => x.AssetId == res.AssetId))))
                {
                    ProgramObj programObj = recommendationSearchResult.AssetType == eAssetTypes.EPG ? epgs.FirstOrDefault(x => x.AssetId == res.AssetId) : recordings.FirstOrDefault(x => x.AssetId == res.AssetId);
                    if (programObj != null)
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
}
