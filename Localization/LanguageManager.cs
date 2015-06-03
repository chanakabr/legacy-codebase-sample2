using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Data;
using System.Threading;
using System.Globalization;
using System.Configuration;
using System.Text.RegularExpressions;
using Tvinci.Configuration;
using Tvinci.MultiClient;
using KLogMonitor;
using System.Reflection;

namespace Tvinci.Localization
{
    public enum eDirection
    {
        LTR,
        RTL
    }

    public enum eLanguageScope
    {
        Request,
        Session
    }

    public class LanguageContext
    {
        public object LanguageID { get; internal set; }
        public object ValueInDB { get; set; }
        public eDirection Direction { get; set; }
        public string Culture { get; set; }
        public DataTable Data { get; set; }
        public CultureInfo CultureInfo { get; private set; }
        public string TVMValue { get; set; }
        public string Name { get; set; }

        public LanguageContext(object languageID, object valueInDB, eDirection direction, string culture, DataTable data)
        {
            if (languageID == null)
            {
                throw new ArgumentNullException("LanguageID");
            }

            LanguageID = languageID;
            ValueInDB = valueInDB;
            Direction = direction;
            Culture = culture;
            CultureInfo = CultureInfo.GetCultureInfo(culture);
            Data = data;

            Data.CaseSensitive = false;
            Data.DefaultView.Sort = "titleID";
        }
    }



    public enum eNotExistsAction
    {
        ShowKey,
        ShowEmptyString,
        ShowHyphen
    }

    public static class LanguageHelper
    {
        public const string CategoryTokenKey = "CategoryToken";
        public const string ItemTokenKey = "titleID";
    }

    public class LanguagesDefinition
    {
        public string DefaultKey { get; set; }
        public string TranslationKey { get; set; }
        public Dictionary<string, LanguageContext> LanguageDictionary { get; private set; }

        public LanguagesDefinition()
        {
            LanguageDictionary = new Dictionary<string, LanguageContext>();
        }
    }





    public abstract class LanguageManager : IDisposable
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public eLanguageScope LanguageScope { get; private set; }
        ReaderWriterLockSlim m_locker = new ReaderWriterLockSlim();
        protected LanguagesDefinition m_LanguageDefinitions;
        public eNotExistsAction NotExistsAction { get; set; }
        public string Identifier { get; private set; }

        static InstanceProvider<LanguageManager> instanceProvider = new InstanceProvider<LanguageManager>();

        public static LanguageManager Instance
        {
            get
            {
                return instanceProvider[MultiClientHelper.Instance.ActiveUserClient.ConfigurationIdentifier];
            }
        }

        public string UserLanguageKey
        {
            get
            {
                string language;
                switch (LanguageScope)
                {
                    case eLanguageScope.Request:
                        language = HttpContext.Current.Items["LanguageManager.ID"] as string;
                        break;
                    case eLanguageScope.Session:
                        language = HttpContext.Current.Session["LanguageManager.ID"] as string;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }



                if (!string.IsNullOrEmpty(language))
                {
                    return language;
                }
                else
                {
                    return m_LanguageDefinitions.DefaultKey;
                }
            }
            private set
            {
                switch (LanguageScope)
                {
                    case eLanguageScope.Request:
                        HttpContext.Current.Items["LanguageManager.ID"] = value;
                        break;
                    case eLanguageScope.Session:
                        HttpContext.Current.Session["LanguageManager.ID"] = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }
        }

        public void RegisterInstance()
        {
            instanceProvider.AddItem(Identifier, this);
        }

        public LanguageManager(string identifier)
        {
            Identifier = identifier;

            NotExistsAction = eNotExistsAction.ShowHyphen;
            string languageScope = ConfigurationManager.AppSettings["Language.UserScope"];

            try
            {
                if (!string.IsNullOrEmpty(languageScope))
                {
                    LanguageScope = (eLanguageScope)Enum.Parse(typeof(eLanguageScope), languageScope);
                }
                else
                {
                    LanguageScope = eLanguageScope.Request;
                }
            }
            catch (Exception e)
            {
                logger.Error("Error occured while tring to set userscope. setting default value 'Request'", e);
                LanguageScope = eLanguageScope.Request;
            }

            logger.InfoFormat("Language scope was set to '{0}'. web.config 'Language.UserScope' value is set to '{1}'", LanguageScope, languageScope);
        }

        #region Properties


        public virtual string this[string token]
        {
            get
            {
                string result = getTranslation(token);
                return result;
            }
            set { }
        }

        public LanguageContext TranslationContext
        {
            get
            {
                LanguageContext context = null;

                if (m_locker.TryEnterReadLock(4000))
                {
                    try
                    {
                        if (m_LanguageDefinitions.LanguageDictionary.TryGetValue(m_LanguageDefinitions.TranslationKey, out context))
                        {
                            return context;
                        }
                        else
                        {
                            throw new Exception(string.Format("Cannot find translation language ID '{0}'", m_LanguageDefinitions.TranslationKey));
                        }
                    }
                    finally
                    {
                        m_locker.ExitReadLock();
                    }
                }

                return null;
            }
        }

        public LanguageContext DefaultContext
        {
            get
            {
                LanguageContext context = null;

                if (m_locker.TryEnterReadLock(4000))
                {
                    try
                    {
                        if (m_LanguageDefinitions.LanguageDictionary.TryGetValue(m_LanguageDefinitions.DefaultKey, out context))
                        {
                            return context;
                        }
                        else
                        {
                            throw new Exception(string.Format("Cannot find default language ID '{0}'", m_LanguageDefinitions.DefaultKey));
                        }
                    }
                    finally
                    {
                        m_locker.ExitReadLock();
                    }
                }

                return null;
            }
        }

        public bool TryGetLanguageByCulture(string culture, out LanguageContext context)
        {
            context = null;

            if (m_locker.TryEnterReadLock(4000))
            {
                try
                {
                    if (m_LanguageDefinitions != null && m_LanguageDefinitions.LanguageDictionary != null && m_LanguageDefinitions.LanguageDictionary.TryGetValue(culture, out context))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                finally
                {
                    m_locker.ExitReadLock();
                }
            }

            return false;
        }

        public LanguageContext UserContext
        {
            get
            {
                LanguageContext context = null;

                if (m_locker.TryEnterReadLock(4000))
                {
                    try
                    {
                        if (m_LanguageDefinitions.LanguageDictionary.TryGetValue(UserLanguageKey, out context))
                        {
                            return context;
                        }
                        else
                        {
                            throw new Exception(string.Format("Cannot find context for language key  '{0}'", UserLanguageKey));
                        }
                    }
                    finally
                    {
                        m_locker.ExitReadLock();
                    }
                }

                return null;
            }
        }
        #endregion

        public void SetActiveLanguageToDefault()
        {
            this.SetActiveLanguageByCulture(string.Empty, true);
        }

        public void SetActiveLanguageByCulture(string culture, bool defaultIfError)
        {
            m_locker.EnterReadLock();
            try
            {
                if (string.IsNullOrEmpty(culture))
                {
                    if (defaultIfError)
                    {
                        culture = m_LanguageDefinitions.DefaultKey;
                    }
                    else
                    {
                        throw new ArgumentNullException("culture");
                    }
                }

                if (m_LanguageDefinitions.LanguageDictionary.ContainsKey(culture))
                {
                    UserLanguageKey = culture;
                }
                else
                {
                    if (defaultIfError)
                    {
                        UserLanguageKey = m_LanguageDefinitions.DefaultKey;
                    }
                    else
                    {
                        throw new Exception("Unknown language id '" + culture + "'");
                    }
                }
            }
            finally
            {
                m_locker.ExitReadLock();
            }
        }

        #region Public methods
        public bool IsDefaultLanguage()
        {
            return (UserLanguageKey.Equals(m_LanguageDefinitions.DefaultKey));
        }

        public bool IsDefaultLanguage(string languageKey)
        {
            return (languageKey.Equals(m_LanguageDefinitions.DefaultKey));
        }

        public void Sync(object parameters)
        {
            if (m_locker.TryEnterWriteLock(4000))
            {
                try
                {
                    m_LanguageDefinitions = null;


                    LanguagesDefinition result = FetchLanguages(parameters);

                    if (!validateDefinitions(result))
                    {
                        throw new Exception("Failed to extract language definitions");

                    }
                    else
                    {
                        m_LanguageDefinitions = result;
                        StringBuilder sb = new StringBuilder();

                        sb.AppendFormat("Default language was set to '{0}'", m_LanguageDefinitions.DefaultKey);
                        sb.AppendLine();
                        sb.AppendFormat("Translation language was set to '{0}'", m_LanguageDefinitions.TranslationKey);
                        sb.AppendLine();

                        foreach (KeyValuePair<string, LanguageContext> item in m_LanguageDefinitions.LanguageDictionary)
                        {
                            int count = (item.Value.Data == null) ? -1 : item.Value.Data.Rows.Count;
                            logger.InfoFormat("Added language of culture '{0}' with {1} translations. site identifier of language '{2}'", item.Value.CultureInfo.TwoLetterISOLanguageName, count, item.Key);
                        }
                    }
                }
                finally
                {
                    m_locker.ExitWriteLock();
                }
            }
        }

        private bool validateDefinitions(LanguagesDefinition result)
        {
            if (result == null)
            {
                logger.Error("Site returned null definitions for languages");
                return false;
            }
            else if (string.IsNullOrEmpty(result.DefaultKey))
            {
                logger.Error("Site default language key was not set.");
                return false;
            }
            else if (result.LanguageDictionary.Count == 0)
            {
                logger.Error("No languages found to be used in site");
                return false;
            }

            logger.InfoFormat("Site supplied valid langauge definitions");
            return true;
        }
        #endregion

        #region Private methods
        private string getTranslation(string key)
        {
            string result = string.Empty;
            LanguageContext context = UserContext;

            if (context.Data != null)
            {
                if (string.IsNullOrEmpty(key))
                {
                    result = getDefault(key);
                    logger.Warn("User tried to get translation with empty key");
                }
                else
                {
                    try
                    {
                        DataRow[] RowsArr = null;
                        Match match = Regex.Match(key, "^(?<Category>[^/]{1}.*)/(?<Key>[^/]+)$");

                        if (match.Success)
                        {
                            RowsArr = context.Data.Select(string.Concat(LanguageHelper.CategoryTokenKey, "='", match.Groups["Category"].Value, "' and ", LanguageHelper.ItemTokenKey, "='", match.Groups["Key"].Value, "'"));
                        }
                        else
                        {
                            int index = context.Data.DefaultView.Find(key);

                            if (index == -1)
                            {
                                result = getDefault(key);
                            }
                            else
                            {
                                result = context.Data.DefaultView[index]["TEXT"].ToString();
                            }

                            return result;
                            //result = context.Data.Rows[index]["TEXT"].ToString(); //context.Data.Select(string.Concat(LanguageHelper.ItemTokenKey, "='", key, "' and ISNULL(", LanguageHelper.CategoryTokenKey, ", '') = ''"));
                        }

                        if (RowsArr == null || RowsArr.Length != 1)
                        {
                            logger.DebugFormat("Failed to find translation for key '{0}' language '{1}'", key, context.Culture);
                            result = getDefault(key);
                        }
                        else
                        {
                            result = RowsArr[0]["TEXT"].ToString();
                        }
                    }
                    catch (Exception e)
                    {
                        result = getDefault(key);
                        logger.Error(string.Format("Error occured while tring to get translation by key '{0}'", key), e);
                    }
                }
            }
            else
            {
                logger.WarnFormat("User tried to get translation of key '{0}' without user LanguageContext", key);
                result = getDefault(key);
            }

            return result;
        }

        protected virtual Int32 GetTVMAccountGroupId()
        {
            return 0;
        }

        protected void setTranslation(string key, string value)
        {
            ODBCWrapper.DataSetSelectQuery mainQuery = null;
            ODBCWrapper.DataSetSelectQuery updateQuery = null;
            ODBCWrapper.DataSetSelectQuery insertQuery = null;
            LanguageContext context = UserContext;
            string ID = String.Empty;
            string MetaDataID = string.Empty;
            try
            {
                if (context.Data != null)
                {
                    do
                    {
                        Int32 nGroupID = GetTVMAccountGroupId();

                        mainQuery = new ODBCWrapper.DataSetSelectQuery();
                        mainQuery += string.Format("select ID from Translation where TitleID = '{0}'", key);
                        if (mainQuery.Execute("Translation", true) == null)
                            break;

                        DataTable table = mainQuery.Table("Translation");
                        // check Translation exists
                        if (table.Rows.Count == 0)//insert new translation
                        {
                            insertQuery = new ODBCWrapper.DataSetSelectQuery();
                            insertQuery += String.Format(@"insert into Translation (TitleID,STATUS,IS_ACTIVE, CategoryToken, GROUP_ID) values('{0}',1,1,'', {1}); select @@Identity as ID;",
                                key, nGroupID == 0 ? String.Empty : nGroupID.ToString());

                            if (insertQuery.Execute("Translation", true) == null)
                                break;

                            DataTable LastInsertedIdTable = insertQuery.Table("Translation");

                            if (table.Rows.Count == 0)
                                break;

                            ID = LastInsertedIdTable.Rows[0]["ID"].ToString();
                        }
                        else//translation exists - update table
                            ID = table.Rows[0]["ID"].ToString();

                        //check Translation MetaData exists
                        mainQuery = new ODBCWrapper.DataSetSelectQuery();
                        mainQuery += string.Format("select ID from TranslationMetadata where TranslationID = '{0}' and Culture = '{1}'", ID, context.Culture);
                        if (mainQuery.Execute("TranslationMetadata", true) == null)
                            break;

                        DataTable metaDatatable = mainQuery.Table("TranslationMetadata");
                        if (metaDatatable.Rows.Count == 0)//Translation MetaData doesn't exists
                        {
                            insertQuery = new ODBCWrapper.DataSetSelectQuery();
                            insertQuery += String.Format(@"insert into TranslationMetadata(TranslationID,LANGUAGE_ID,Status, Text,CREATE_DATE,UPDATE_DATE,UPDATER_ID, Culture,OriginalText,GROUP_ID,IS_ACTIVE)
                                                            select top 1 {0},lul.ID,1,'{1}','{2}','{2}',0,'{3}','{1}','{4}',1 from lu_languages lul where CULTURE = '{3}' ; select @@Identity as ID;",
                                                            ID, value, DateTime.Now.ToString(), context.Culture, nGroupID == 0 ? String.Empty : nGroupID.ToString());

                            if (insertQuery.Execute("TranslationMetadata", true) == null)
                                break;

                            DataTable LastInsertedIdTable = insertQuery.Table("TranslationMetadata");

                            if (table.Rows.Count == 0)
                                break;

                            ID = LastInsertedIdTable.Rows[0]["ID"].ToString();

                            context.Data.LoadDataRow(new object[] { string.Empty, key, value }, true);
                        }
                        else
                        {
                            MetaDataID = metaDatatable.Rows[0]["ID"].ToString();
                            updateQuery = new ODBCWrapper.DataSetSelectQuery();

                            updateQuery += String.Format(@"update TranslationMetadata
                                                       set Status = 1,
                                                           LANGUAGE_ID = (select top 1 ID from lu_languages where CULTURE = '{1}' ),
                                                           Text = '{2}',
                                                           OriginalText = '{2}',
                                                           UPDATE_DATE = '{3}',
                                                           GROUP_ID = {4}
                                                        where ID = '{0}'", MetaDataID, context.Culture, value, DateTime.Now.ToString(), (nGroupID == 0 ? "null" : nGroupID.ToString()));
                            if (updateQuery.Execute("TranslationMetadata", true) == null)
                            {
                                DataRow[] RowsArr = context.Data.Select(string.Concat(LanguageHelper.ItemTokenKey, " = '", key, "'"));
                                if (RowsArr.Length > 0)
                                    RowsArr[0]["TEXT"] = value;
                                break;
                            }
                        }
                    }
                    while (false);
                }
            }
            finally
            {
                if (mainQuery != null)
                {
                    mainQuery.Finish();
                    mainQuery = null;
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                    updateQuery = null;
                }
                if (insertQuery != null)
                {
                    insertQuery.Finish();
                    insertQuery = null;
                }
            }
        }


        private string getDefault(string key)
        {
            string result;

            switch (NotExistsAction)
            {
                case eNotExistsAction.ShowKey:
                    result = string.Format("{{{{{0}}}}}", key);
                    break;
                case eNotExistsAction.ShowEmptyString:
                    result = string.Empty;
                    break;
                case eNotExistsAction.ShowHyphen:
                default:
                    result = "-";
                    break;
            }

            return result;
        }
        #endregion

        #region Abstract methods
        protected abstract LanguagesDefinition FetchLanguages(object parameters);
        #endregion

        public List<LanguageContext> GetLanguages()
        {
            m_locker.EnterReadLock();
            try
            {
                return m_LanguageDefinitions.LanguageDictionary.Values.ToList<LanguageContext>();
            }
            finally
            {
                m_locker.ExitReadLock();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                instanceProvider.RemoveItem(Identifier);
            }
            catch
            {
                // empty by design                
            }

        }

        #endregion
    }
}
