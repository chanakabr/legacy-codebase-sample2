using System;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Tvinci.Web.HttpModules.Configuration;
using System.Collections.Specialized;
using Tvinci.Helpers.Link.Configuration;
using KLogMonitor;
using System.Reflection;

namespace Tvinci.Helpers
{
    #region Enum
    [Flags]
    public enum eCreateOptions
    {
        None = 0,
        CopyExists = 2,
        LeaveRelativeURL = 4,
        ForceHandleSpecialTypes = 8
    }

    #endregion

    #region QueryStringHelper

    /// <summary>
    /// Summary description for QuerystringHelper
    /// </summary>
    public class QueryStringHelper
    {
        #region Constructor
        private QueryStringHelper()
        {

        }

        static QueryStringHelper()
        {
            Instance = new QueryStringHelper();
        }
        #endregion

        #region Enum
        public enum eNotValidAction
        {
            Response404,
            Exception
        }

        private enum eViolationTreatment
        {
            Remove,
            Exception,
            ConvertToBase64
        }
        #endregion

        #region Private Fields
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        private QueryStringCollection ContextParameters
        {
            get
            {
                QuerystringContext result = HttpContext.Current.Items["ContextParameters"] as QuerystringContext;

                if (result == null)
                {
                    result = new QuerystringContext();
                    HttpContext.Current.Items["ContextParameters"] = result;
                }

                if (result.Query != HttpContext.Current.Request.QueryString.ToString())
                {
                    result.Query = HttpContext.Current.Request.QueryString.ToString();

                    result.ContextParameters.Clear();

                    for (int i = 0; i < HttpContext.Current.Request.QueryString.Count; i++)
                    {
                        string key = HttpContext.Current.Request.QueryString.Keys[i];
                        string value = HttpContext.Current.Request.QueryString[i];
                        handleQueryItem(result.ContextParameters, key, value.Replace("+", "%2B"), eItemType.Clear, eViolationTreatment.Exception);
                    }
                }

                return result.ContextParameters;
            }
        }
        #endregion

        #region Static Fields
        internal static QueryStringHelper Instance = null;

        public static AddQueryToURLDelegate HandleLanguageMethod { get; set; }
        #endregion

        #region Public Static Methods
        public static string CreateQueryString(string url, params QueryStringPair[] items)
        {
            return CreateQueryString(url, eCreateOptions.None, items);
        }

        /// <summary>
        /// Creates url link with querystring. 
        /// </summary>
        /// <param name="url">Can be String.Empty which represents current url</param>
        /// <param name="createOptions"></param>
        /// <param name="items">Passing string without ';' will clone the current value of the requested key</param>
        /// <returns></returns>
        public static string CreateQueryString(string url, eCreateOptions createOptions, params QueryStringPair[] items)
        {
            return CreateQueryString(url, createOptions, null, items);
        }

        public static string CreateQueryString(string url, eCreateOptions createOptions, string[] queryItemToRemove, params QueryStringPair[] items)
        {
            QueryStringCollection linkQuerystringItems = new QueryStringCollection();

            // Extract current query items
            if (url.Contains("?"))
            {
                handleQuery(linkQuerystringItems, url.Substring(url.IndexOf('?') + 1), eItemType.Clear, eViolationTreatment.ConvertToBase64);
            }

            // Handle language query item
            QueryStringPair languageQuery = null;
            if (HandleLanguageMethod != null)
            {
                languageQuery = HandleLanguageMethod();

                if (languageQuery != null)
                {
                    if (!string.IsNullOrEmpty(languageQuery.Value) && !linkQuerystringItems.ContainsKey(languageQuery.Key))
                    {
                        linkQuerystringItems[languageQuery.Key] = languageQuery;
                    }
                }
            }

            QueryStringConfig data = QueryConfigManager.Instance.Data;

            // Add query items
            foreach (QueryStringPair item in items)
            {
                item.ItemType = data.General.Base64Mode ? eItemType.Base64 : eItemType.Clear;
                linkQuerystringItems[item.Key] = item;
            }

            // Create base url
            if (string.IsNullOrEmpty(url))
            {
                url = HttpContext.Current.Request.Path;
            }
            else
            {
                url = LinkHelper.GetLinkWithoutQuerystring(url);
            }

            // Copy existing query string
            if ((createOptions & eCreateOptions.CopyExists) == eCreateOptions.CopyExists)
            {
                foreach (KeyValuePair<string, QueryStringPair> item in Instance.ContextParameters)
                {
                    if (languageQuery != null && item.Key == languageQuery.Key)
                    {
                        // prevent adding language when the in defualt language
                        continue;
                    }

                    if (!linkQuerystringItems.ContainsKey(item.Key))
                    {
                        linkQuerystringItems.Add(item.Key, item.Value);
                    }
                }
            }

            // Remove query items if given
            if (queryItemToRemove != null)
            {
                foreach (string rItem in queryItemToRemove)
                {
                    if (linkQuerystringItems.ContainsKey(rItem))
                    {
                        linkQuerystringItems.Remove(rItem);
                    }
                }
            }

            // Create url with query items
            string result = string.Empty;
            if ((createOptions & eCreateOptions.ForceHandleSpecialTypes) == eCreateOptions.ForceHandleSpecialTypes)
            {
                // force the application to handle special types
                result = linkQuerystringItems.GenerateLink(url, QueryStringCollection.eMode.All);
            }
            else
            {
                // let the infrastructure to decide wether to handle special types
                result = linkQuerystringItems.GenerateLink(url);
            }

            // Make url absolute if needed
            if ((createOptions & eCreateOptions.LeaveRelativeURL) != eCreateOptions.LeaveRelativeURL)
            {
                result = LinkHelper.MakeAbsolute(LinkHelper.GetDefaultUri(), result);
            }

            return result;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static string VerifyEncrypted(string url)
        {
            if (!QueryConfigManager.Base64Mode)
            {
                return url;
            }

            try
            {
                QueryStringCollection queryItems = new QueryStringCollection();
                handleQuery(queryItems, LinkHelper.GetQuerystring(url), eItemType.Clear, eViolationTreatment.ConvertToBase64);
                queryItems.GenerateLink(url);
                return url;
            }
            catch
            {
                return string.Empty;
                //return "javascript:void(0);";                                
            }
        }

        internal static string EncryptString(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value)).TrimEnd('=');
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static string EncryptQueryString(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            string[] token = url.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries);

            if (token.Length == 2)
            {
                return string.Concat(token[0], "?", EncryptString(token[1]));
            }
            else if (token.Length == 1)
            {
                return url;
            }
            else
            {
                throw new Exception(string.Format("Invalid url (multiple '?' signs). url '{0}'", url));
            }
        }

        public static string DecryptQueryString(string querystring)
        {
            querystring = LinkHelper.ActualUrlDecode(querystring).TrimEnd('=').Replace(" ", "+");
            int remainder = querystring.Length % 4;

            if (remainder != 0)
            {
                querystring = querystring.PadRight((querystring.Length + (4 - remainder)), '=');
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(querystring));
        }

        /// <summary>
        /// Returns value of querystring parameter. if value exists returns true and the value as out parameter
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetString(string key, out string value)
        {
            QueryStringPair result;
            if (Instance.ContextParameters.TryGetValue(key.ToLower(), out result))
            {
                value = result.Value;
                return true;
            }
            else
            {
                value = string.Empty;
                return false;
            }
        }

        /// <summary>
        /// Returns value of querystring parameter. If parameter not found returns the default value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetString(string key)
        {
            return GetString(key, string.Empty);
        }

        /// <summary>
        /// Returns value of querystring parameter. If parameter not found returns the default value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string GetString(string key, string defaultValue)
        {
            string result;

            if (TryGetString(key, out result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Returns value of querystring parameter. if value exists returns true and the value as out parameter.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryGetValue<TValue>(string key, out TValue value)
        {
            value = default(TValue);

            string tempValue;
            if (TryGetString(key, out tempValue))
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(TValue));

                if (converter.CanConvertTo(typeof(TValue)))
                {
                    value = (TValue)TypeDescriptor.GetConverter(typeof(TValue)).ConvertTo(tempValue, typeof(TValue));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns value of querystring parameter.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue GetValue<TValue>(string key, TValue defaultValue)
        {
            string tempValue;

            try
            {
                if (TryGetString(key, out tempValue))
                {
                    if (typeof(TValue) == typeof(bool))
                    {
                        object value = bool.Parse(tempValue);
                        return (TValue)value;
                    }
                    else
                    {
                        return (TValue)TypeDescriptor.GetConverter(typeof(TValue)).ConvertTo(tempValue, typeof(TValue));
                    }
                }
            }
            catch (Exception)
            {
                logger.ErrorFormat("Attempting to convert value to querystring items collection with the wrong type. item is being ignored. type: '{0}', value '{1}'", typeof(TValue).ToString(), key);
            }

            return defaultValue;
        }

        public static object GetObject<TValue>(string key, object defaultValue)
        {
            string tempValue;

            if (TryGetString(key, out tempValue))
            {
                if (typeof(TValue) == typeof(bool))
                {
                    object value = bool.Parse(tempValue);
                    return (TValue)value;
                }
                else
                {
                    return (TValue)TypeDescriptor.GetConverter(typeof(TValue)).ConvertTo(tempValue, typeof(TValue));
                }
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Returns value of querystring parameter.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static TValue GetEnum<TValue>(string key, TValue defaultValue)
        {
            string tempValue;

            if (TryGetString(key, out tempValue))
            {
                if (Enum.IsDefined(typeof(TValue), tempValue))
                {
                    return (TValue)Enum.Parse(typeof(TValue), tempValue, true);
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }

        }

        /// <summary>
        /// Returns true if the query string has a value for the key
        /// </summary>
        public static bool HasValue(string key)
        {
            return !string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[key]);
        }

        public static bool IsQueryValid(bool treatPageConfiguration, params string[] parameters)
        {
            return IsQueryValid(treatPageConfiguration, false, parameters);
        }

        public static bool IsQueryValid(bool treatPageConfiguration, bool compareParametersList, params string[] parameters)
        {
            List<string> parametersList = new List<string>(parameters);

            if (compareParametersList)
            {
                if (Instance.ContextParameters.Count != parametersList.Count)
                {
                    return false;
                }

                foreach (string key in Instance.ContextParameters.Keys)
                {
                    if (!parametersList.Contains(key, compareInstance))
                    {
                        return false;
                    }
                }
            }

            foreach (string key in Instance.ContextParameters.Keys)
            {
                if (!parametersList.Contains(key, compareInstance))
                {
                    if (treatPageConfiguration)
                    {
                        string pattern;
                        if (!QueryConfigManager.Instance.TryGetRule(LinkHelper.GetPageVirtualPath(), key, out pattern))
                        {
                            if (!QueryConfigManager.Instance.TryGetRule(key, out pattern))
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public static void ValidateQuery(bool treatPageConfiguration, eNotValidAction action, params string[] parameters)
        {
            if (!IsQueryValid(treatPageConfiguration, parameters))
            {
                switch (action)
                {
                    case eNotValidAction.Response404:
                        HttpContext.Current.Response.StatusCode = 404;
                        HttpContext.Current.Response.End();
                        return;
                    case eNotValidAction.Exception:
                    default:
                        throw new Exception("Querystring contains not supported keys");
                }
            }
        }

        public static string RemoveQuerystring(string url)
        {
            int index = url.IndexOf('?');

            if (index == -1)
            {
                return url;
            }

            return url.Substring(0, index);
        }
        #endregion

        #region Private Static Methods
        private static CompareCaseInSensitive compareInstance = new CompareCaseInSensitive();

        private static void handleQuery(QueryStringCollection collection, string querystring, eItemType itemType, eViolationTreatment violationTreatment)
        {
            if (string.IsNullOrEmpty(querystring))
            {
                // no query found
                return;
            }

            string[] querystringArray = querystring.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string queryItem in querystringArray)
            {
                string key;
                string value = string.Empty;

                if (queryItem.IndexOf('=') == -1)
                {
                    key = string.Empty;
                    value = queryItem;
                }
                else
                {
                    key = queryItem.Substring(0, queryItem.IndexOf('='));
                    if (queryItem.IndexOf('=') != queryItem.Length - 1)
                    {
                        value = queryItem.Substring(queryItem.IndexOf('=') + 1);
                    }
                }

                handleQueryItem(collection, key, value, itemType, violationTreatment);
            }
        }

        private static void handleQueryItem(QueryStringCollection collection, string key, string value, eItemType itemType, eViolationTreatment violationTreatment)
        {
            QueryStringConfig data = QueryConfigManager.Instance.Data;

            if (data.General.Base64Mode &&
                ((string.IsNullOrEmpty(key)) || data.Base64.Key.ToLower() == key.ToLower()))
            {
                // try to decrypt
                try
                {
                    value = DecryptQueryString(value);
                    handleQuery(collection, value, eItemType.Base64, violationTreatment);

                }
                catch (Exception ex)
                {
                    logger.Info(string.Format("Error occured while tring to decrypt query item '{0}'", value), ex);
                    throw;
                }

                return;
            }
            else
            {
                bool shouldValidate;

                switch (data.General.ValidateMode)
                {
                    case ValidateMode.Never:
                        shouldValidate = false;
                        break;
                    case ValidateMode.Always:
                        shouldValidate = true;
                        break;
                    case ValidateMode.ClearOnly:
                        shouldValidate = (itemType == eItemType.Clear);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                    // break;
                }

                if (shouldValidate && !QueryConfigManager.Instance.IsPageIgnored(LinkHelper.GetPageVirtualPath()))
                {
                    string pattern;
                    string errorMessage = string.Empty;
                    // check if exists rule on page scope
                    if (!QueryConfigManager.Instance.TryGetRule(LinkHelper.GetPageVirtualPath(), key, out pattern))
                    {
                        // check if exists rule on global scope
                        if (!QueryConfigManager.Instance.TryGetRule(key, out pattern))
                        {
                            errorMessage = string.Format("The key '{0}' is not allowed to be used without base64 encryption", key);
                        }
                    }

                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        if (!Regex.IsMatch(value, pattern, RegexOptions.Singleline))
                        {
                            errorMessage = string.Format("The key '{0}' value '{1}' doesn't meet the rule '{2}'", key, value, pattern);
                        }
                    }

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        switch (violationTreatment)
                        {
                            case eViolationTreatment.Remove:
                                return;
                            case eViolationTreatment.Exception:
                                logger.Info(errorMessage);
                                throw new Exception(errorMessage);
                            case eViolationTreatment.ConvertToBase64:
                                itemType = eItemType.Base64;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("violationTreatment");
                        }
                    }
                }


                if (!string.IsNullOrEmpty(key))
                {
                    QueryStringPair newItem = new QueryStringPair(key, LinkHelper.ActualUrlDecode(value), itemType);
                    collection[key.ToLower()] = newItem;
                }
                else
                {
                    logger.ErrorFormat("Attempting to add value without a key to querystring items collection. item is being ignored. value '{0}'", value);
                }
            }
        }
        #endregion

        #region QuerystringContext
        private class QuerystringContext
        {
            public QueryStringCollection ContextParameters { get; private set; }
            public string Query { get; set; }

            public QuerystringContext()
            {
                ContextParameters = new QueryStringCollection();
                Query = string.Empty;
            }
        }
        #endregion

        #region Delegates
        public delegate QueryStringPair AddQueryToURLDelegate();
        #endregion
    }
    #endregion
}
