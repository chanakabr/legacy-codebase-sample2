using System.Collections.Generic;
using System.Linq;
using ApiObjects;

namespace ApiLogic.IndexManager.Helpers
{
    /// <summary>
    /// keeps all the common static method that can be used between the different index managers
    /// </summary>
    public static class IndexManagerCommonHelpers
    {
        public static string GetTranslationType(string type, LanguageObj language)
        {
            if (language.IsDefault)
            {
                return type;
            }
            
            return string.Concat(type, "_", language.Code);
        }
        
        public static List<string> GetEpgsCBKeysV1(IEnumerable<long> epgIds, IEnumerable<LanguageObj> langCodes)
        {
            var result = new List<string>();
            if (langCodes == null)
            {
                result = epgIds.Select(x => x.ToString()).ToList();
            }
            else
            {
                foreach (var epgId in epgIds)
                {
                    var keys = langCodes.Select(langCode => langCode.IsDefault ? epgId.ToString() : $"epg_{epgId}_lang_{langCode.Code.ToLower()}");

                    result.AddRange(keys.ToList());
                }
            }

            return result;
        }
    }
}