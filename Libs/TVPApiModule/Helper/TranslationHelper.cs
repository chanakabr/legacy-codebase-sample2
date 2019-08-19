using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApiModule.Objects;
using TVPApiModule.Manager;
using Tvinci.Localization;
using System.Data;
using TVPApi;

namespace TVPApiModule.Helper
{
    public class TranslationHelper
    {
        public static Pair[] GetTranslations(int groupID, TVPApi.PlatformType platform)
        {
            
            List<LanguageContext> languages = TextLocalizationManager.Instance.GetTextLocalization(groupID, platform).GetLanguages();
            Pair[] retTranslations = new Pair[languages.Count];
            int i = 0;

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
                retTranslations[i] = new Pair(lang.Culture, TranslationsList);
                i++;
            }

            return retTranslations;
        }
    }

}
