using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Localization;
using System.Data;
using TVPPro.SiteManager.Helper;
using TVPPro.Configuration.Technical;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.Manager
{
    public class TextLocalization : LanguageManager
    {
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

        private TextLocalization()
            : base(string.Empty)
        {
            TranslationCulture = string.Empty;
        }

        static TextLocalization instance = new TextLocalization();
        //public static void CreateInstance(string connectionString)
        //{
        //    new TextLocalization();
        //}

        public static TextLocalization Instance
        {
            get
            {
                return instance;
            }
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
            TVMAccountType account = PageData.Instance.GetTVMAccountByUserName(TechnicalConfiguration.Instance.TVMConfiguration.User);
            return account.GroupID;
        }

        protected override LanguagesDefinition FetchLanguages(object parameters)
        {
            //string connectionString = (parameters is string) ? (string) parameters : string.Empty;

            LanguagesDefinition result = new LanguagesDefinition();
            result.TranslationKey = this.TranslationCulture;

            logger.Info("Start syncing site languages");
            ODBCWrapper.DataSetSelectQuery mainQuery = null;

            try
            {
                mainQuery = new ODBCWrapper.DataSetSelectQuery();

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
                            string name = oRow["NAME"] as string;

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
                                ODBCWrapper.DataSetSelectQuery subQuery = new ODBCWrapper.DataSetSelectQuery();
                                subQuery += "select t.CategoryToken '" + LanguageHelper.CategoryTokenKey + "', t.titleID '" + LanguageHelper.ItemTokenKey + "', dbo.isempty(tm.TEXT, tm.OriginalText) 'TEXT' ";
                                subQuery += "from translationMetadata tm left join translation t on t.id = tm.translationid where ";
                                subQuery += ODBCWrapper.Parameter.NEW_PARAM("tm.Culture", "=", culture);

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

                            result.LanguageDictionary.Add(key, new LanguageContext(key, valueInDB, direction, culture, languageData) { TVMValue = tvmValue });
                        }
                        catch (Exception ex)
                        {
                            logger.Error("Error occured while extracting language information ", ex);
                            //logger.Fatal(string.Format("Failed to handle language id '{0}' name '{1}'", oRow["ID"], oRow["Name"]), ex);
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
    }
}
