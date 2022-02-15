using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Localization;
using System.Data;
using TVPApi;
using Phx.Lib.Log;
using System.Reflection;

namespace TVPApiModule.Objects
{
    public class TextLocalization : LanguageManager
    {
        private int m_GroupID;
        private PlatformType m_Platform;

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public string TranslationCulture { get; set; }

        public delegate string AddTokentitleId(string key, string translation);
        protected event AddTokentitleId AddTokenWrapperHandler;

        public void AttachEventHandler(AddTokentitleId evtHandler)
        {
            AddTokenWrapperHandler -= evtHandler;
            AddTokenWrapperHandler += evtHandler;
        }
        public void ResetEventHandler()
        {
            AddTokenWrapperHandler = null;
        }

        public TextLocalization(int groupID, PlatformType platform)
            : base(string.Empty)
        {
            TranslationCulture = string.Empty;
            m_GroupID = groupID;
            m_Platform = platform;
            Sync(null);
        }

        [Obsolete("use [] instead")]
        public string GetText(string key)
        {
            return this[key];
        }

        public override string this[string token]
        {
            get
            {
                string result = base[token];
                if (AddTokenWrapperHandler != null)
                    result = AddTokenWrapperHandler(token, result);
                return result;
            }
            set
            {
                setTranslation(token, value);
            }
        }

        protected override int GetTVMAccountGroupId()
        {
            return m_GroupID;
        }

        protected override LanguagesDefinition FetchLanguages(object parameters)
        {
            ConnectionManager conManager = new ConnectionManager(m_GroupID, m_Platform, false);
            string connectionString = conManager.GetClientConnectionString();

            LanguagesDefinition result = new LanguagesDefinition();
            result.TranslationKey = this.TranslationCulture;

            logger.Info("Start syncing site languages");
            TVPApi.ODBCWrapper.DataSetSelectQuery mainQuery = null;

            try
            {
                mainQuery = new TVPApi.ODBCWrapper.DataSetSelectQuery();
                mainQuery.SetConnectionString(connectionString);
                string translationPart = string.IsNullOrEmpty(TranslationCulture) ? "" : string.Format("or culture = '{0}'", TranslationCulture);
                mainQuery += string.Format("select ID, CULTURE, NAME, DIRECTION, isUsed, isDefault, TVMValue from LU_LANGUAGES where (isUsed = 1 {0})", translationPart);

                if (mainQuery.Execute("temp", true) != null)
                {
                    DataTable table = mainQuery.Table("temp");

                    logger.InfoFormat("Extracted {0} languages.", table.Rows.Count);

                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        DataRow oRow = table.Rows[i];

                        try
                        {
                            string tvmValue = oRow["TVMValue"] as string;
                            string culture = (string)oRow["Culture"];
                            string name = (string)oRow["Name"];

                            logger.DebugFormat("Found language with culture '{0}', is default '{1}', is used '{2}'", oRow["Culture"], oRow["isDefault"], oRow["IsUsed"]);

                            if (!isValidCulture(culture))
                            {
                                logger.ErrorFormat("Failed to handle language id '{0}' name '{1}'. Language is not supported (Did you forget to add the language culture '{2}' to the enum 'eLanguage'?)", oRow["ID"], oRow["Name"], oRow["Culture"]);
                                continue;
                            }

                            string key = culture;

                            object valueInDB = oRow["ID"];
                            eDirection direction = (eDirection)Enum.Parse(typeof(eDirection), (string)oRow["Direction"]);

                            if ((bool)oRow["isDefault"])
                            {
                                if (string.IsNullOrEmpty(result.TranslationKey))
                                {
                                    // no custom translation value set. using default language
                                    result.TranslationKey = key.ToString();
                                }

                                result.DefaultKey = key.ToString();
                            }

                            DataTable languageData = null;
                            if ((bool)oRow["IsUsed"])
                            {
                                TVPApi.ODBCWrapper.DataSetSelectQuery subQuery = new TVPApi.ODBCWrapper.DataSetSelectQuery();
                                subQuery.SetConnectionString(connectionString);
                                subQuery += "select t.CategoryToken '" + LanguageHelper.CategoryTokenKey + "', t.titleID '" + LanguageHelper.ItemTokenKey + "', ISNULL(NULLIF(tm.TEXT, ''), tm.OriginalText) 'TEXT' ";
                                subQuery += "from translationMetadata tm left join translation t on t.id = tm.translationid where ";
                                subQuery += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("tm.Culture", "=", culture);

                                logger.DebugFormat("Trying to extract translation for culture '{0}'", culture);
                                string tableName = oRow["CULTURE"].ToString();
                                DataTable transTable = subQuery.Execute(tableName, true);
                                if (transTable != null)
                                {
                                    languageData = transTable.Copy();
                                }
                                else
                                {
                                    logger.ErrorFormat("Failed to execute sql against db to extract translations for culture '{0}'", culture);
                                }

                                subQuery.Finish();
                            }

                            result.LanguageDictionary.Add(key, new LanguageContext(key, valueInDB, direction, culture, languageData) { TVMValue = tvmValue, Name = name });
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error occured while extracting language information ", ex);
                        }
                    }
                }
                else
                {
                    logger.Error("Failed to execute query against db to retrieve language information");
                }
            }
            finally
            {
                if (mainQuery != null)
                {
                    mainQuery.Finish();
                }
                logger.Info("Finished syncing site languages");
            }

            return result;
        }

        private bool isValidCulture(string culture)
        {
            return (culture.Length == 2);
        }

        public int GetLanguageDBID(string culture)
        {
            var languages = GetLanguages();
            culture = string.IsNullOrEmpty(culture) ? DefaultContext.Culture : culture;
            if (!string.IsNullOrEmpty(culture) && languages != null && languages.Count > 0)
            {
                LanguageContext retLang = languages.Where(lang => lang.Culture == culture).FirstOrDefault();
                if (retLang != null)
                {
                    return int.Parse(retLang.ValueInDB.ToString());
                }
            }
            return 0;
        }

    }
}
