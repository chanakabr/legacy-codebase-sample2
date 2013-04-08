using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApiModule.Objects;
using TVPApiModule.Manager;
using Tvinci.Localization;
using System.Data;

namespace TVPApiModule.Helper
{
    public class TranslationHelper
    {
        public static Dictionary<string, List<Translation>> GetTranslations(int groupID, TVPApi.PlatformType platform)
        {
            Dictionary<string, List<Translation>> retTranslations = new Dictionary<string, List<Translation>>();
            List<LanguageContext> languages = TextLocalizationManager.Instance.GetTextLocalization(groupID, platform).GetLanguages();

            foreach (LanguageContext lang in languages)
            {
                DataTable translationsTable = lang.Data;
                List<Translation> TranslationsList = new List<Translation>();
                foreach (DataRow row in translationsTable.Rows)
                {
                    Translation translation = new Translation()
                    {
                        Culture = lang.Culture,
                        LanguageID = int.Parse(lang.ValueInDB.ToString()),
                        OriginalText = row["Text"].ToString(),
                        TitleID = row["titleID"].ToString()
                    };
                    TranslationsList.Add(translation);
                }
                retTranslations.Add(lang.Culture, TranslationsList);
            }

            return retTranslations;
        }
    }
}
