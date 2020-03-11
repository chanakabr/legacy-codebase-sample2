using System;
using System.Web;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using Tvinci.Helpers.Link.Configuration;
using Tvinci.Web.HttpModules.Configuration;
using TVPPro.Configuration.Technical;
using KLogMonitor;
using System.Reflection;

namespace Tvinci.Helpers
{
    public static class LinkHelper
    {
        /// <summary>        
        /// </summary>
        /// <remarks>
        /// the trim will handle website with virtual directory. 
        /// if no virtual directory then the 'request.ApplicationPath' will return '/'
        /// </remarks>
        /// <returns></returns>
        public static string GetPageVirtualPath()
        {
            return HttpContext.Current.Request.FilePath.Remove(0, HttpContext.Current.Request.ApplicationPath.Length).TrimStart('/');
        }

        #region Constructor
        static LinkHelper()
        {
            bool UsePermanentURL = false;
            try
            {
                UsePermanentURL = TechnicalConfiguration.Instance.Data.Site.ApplicativeBaseUri.UsePermanentURL;
                logger.Info(string.Format("'UsePermanentURL' value is '{0}'", UsePermanentURL));
            }
            catch (Exception)
            {
                logger.Info(string.Format("'UsePermanentURL' value is '{0}'", UsePermanentURL));
            }
            if (UsePermanentURL)
            {
                try
                {
                    //if (string.IsNullOrEmpty(uriFromConfiguration))
                    //{
                    //    logger.InfoFormat("URI not set in web.config. extracting from current request '{0}'", HttpContext.Current.Request.Url.OriginalString);

                    //    string pathAndQuery = HttpContext.Current.Server.UrlDecode(HttpContext.Current.Request.Url.PathAndQuery);
                    //    logger.DebugFormat("Original requst string '{0}' | PathAndQuery '{1}'", HttpContext.Current.Request.Url.OriginalString, pathAndQuery);
                    //    uriFromConfiguration = HttpContext.Current.Request.Url.OriginalString.Substring(0, HttpContext.Current.Request.Url.OriginalString.Length - pathAndQuery.Length);
                    //    logger.DebugFormat("Calculated base '{0}' | application path '{1}'", uriFromConfiguration, HttpContext.Current.Request.ApplicationPath);
                    //    uriFromConfiguration = string.Format("{0}{1}", uriFromConfiguration, HttpContext.Current.Request.ApplicationPath);

                    //}

                    string uriFromConfiguration = TechnicalConfiguration.Instance.Data.Site.ApplicativeBaseUri.BaseUri;
                    string uriSecureFromConfiguration = TechnicalConfiguration.Instance.Data.Site.ApplicativeBaseUri.SecureBaseUri;
                    if (string.IsNullOrEmpty(uriFromConfiguration) || string.IsNullOrEmpty(uriSecureFromConfiguration))
                    {
                        applicativeBaseUri = null;
                        applicativeSecureBaseUri = null;
                        return;
                    }

                    // baseURI
                    if (uriFromConfiguration.Trim().EndsWith("."))
                    {
                        uriFromConfiguration = uriFromConfiguration.TrimEnd('.');
                    }

                    if (!uriFromConfiguration.Trim().EndsWith("/"))
                    {
                        uriFromConfiguration = string.Format("{0}/", uriFromConfiguration);
                    }

                    // secureBaseURI
                    if (uriSecureFromConfiguration.Trim().EndsWith("."))
                    {
                        uriSecureFromConfiguration = uriSecureFromConfiguration.TrimEnd('.');
                    }

                    if (!uriSecureFromConfiguration.Trim().EndsWith("/"))
                    {
                        uriSecureFromConfiguration = string.Format("{0}/", uriSecureFromConfiguration);
                    }

                    logger.InfoFormat("resulted applicative uri '{0}'", uriFromConfiguration);
                    applicativeBaseUri = new Uri(uriFromConfiguration);
                    applicativeSecureBaseUri = new Uri(uriSecureFromConfiguration);
                }
                catch (Exception ex)
                {
                    applicativeBaseUri = null;
                    applicativeSecureBaseUri = null;
                    logger.Error("Failed to calculate applicativeBaseUri", ex);
                }
            }
        }
        #endregion

        #region Fields
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Uri applicativeBaseUri = null;
        private static readonly Uri applicativeSecureBaseUri = null;
        #endregion

        #region Static Methods
        public static string ParseURL(object relativeURL)
        {
            if (relativeURL is string)
            {
                return ParseURL(null, (string)relativeURL);
            }
            else
            {
                return string.Empty;
            }


        }



        [Obsolete("Use 'ParseURL(Uri baseUri, string url)' instead")]
        public static string ParseURL(Uri baseUri, string url, bool forceInnerURLHandling)
        {
            return ParseURL(baseUri, url);
        }

        public static string ParseURL(Uri baseUri, string url)
        {
            if (url.Contains(".aspx"))
            {
                if (IsBaseOfApplication(MakeAbsolute(baseUri, url)))
                {
                    return QueryStringHelper.CreateQueryString(url);
                }
                else
                {
                    if (baseUri == null)
                    {
                        return url;
                    }
                    else
                    {
                        return MakeAbsolute(baseUri, url);
                    }
                }
            }
            else
            {
                return MakeAbsolute(baseUri, url);
            }
        }


        public static string MakeAbsolute(Uri baseUri, string relativeURL)
        {
            Uri result;

            relativeURL = relativeURL.TrimEnd();

            // ************************************************************************************
            // WORKAROUND!!!!! should be replaced on version 1.4.0.0
            if (relativeURL == "#")
            {
                return "#";
            }
            // ************************************************************************************

            if (baseUri == null)
            {
                baseUri = GetDefaultUri();
            }
            else if (!baseUri.AbsoluteUri.EndsWith("/"))
            {
                baseUri = new Uri(string.Concat(baseUri.AbsoluteUri, "/"));
            }

            if (relativeURL.ToLower().StartsWith("http"))
            {
                result = new Uri(relativeURL);
            }
            else if (relativeURL.ToLower().StartsWith("www."))
            {
                result = new Uri(string.Format("http://{0}", relativeURL));
            }
            else
            {
                if (relativeURL.Equals("~"))
                {
                    relativeURL = "~/";
                }

                if (relativeURL.StartsWith("~"))
                {
                    result = (new Uri(baseUri, relativeURL.Remove(0, 2)));
                }
                else if (relativeURL.StartsWith("."))
                {
                    // relative to current location
                    result = (new Uri(HttpContext.Current.Request.Url, relativeURL));
                }
                else
                {
                    // calculate from argumented uri
                    result = (new Uri(baseUri, relativeURL));
                }
            }

            return result.AbsoluteUri;
        }

        public static bool IsBaseOf(Uri baseUri, Uri uri)
        {
            return (new Uri(baseUri.AbsoluteUri.ToLower()).IsBaseOf(new Uri(uri.AbsoluteUri.ToLower())));
        }

        public static bool IsBaseOfApplication(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                return true;
            }
            else
            {
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    return IsBaseOf(GetDefaultUri(), new Uri(url));
                }
                else
                {
                    return false;
                }
            }
        }

        public static Uri GetActualUri()
        {
            Uri newUri;

            string appPath = HttpContext.Current.Request.ApplicationPath.EndsWith("/") ? HttpContext.Current.Request.ApplicationPath : string.Concat(HttpContext.Current.Request.ApplicationPath, "/");

            if (Uri.TryCreate(HttpContext.Current.Request.Url, appPath, out newUri))
            {
                return newUri;
            }
            else
            {
                return null;
            }
        }

        public static Uri GetDefaultUri()
        {
            Uri uriRet = (HttpContext.Current.Request.IsSecureConnection)? applicativeSecureBaseUri : applicativeBaseUri;

            if (uriRet != null)
            {
                return uriRet;
            }
            else
            {
                return GetActualUri();
            }
        }

        public static string GetActualPage()
        {
            string sURL = HttpContext.Current.Request.CurrentExecutionFilePath;
            Int32 nStart = sURL.LastIndexOf('/') + 1;
            Int32 nEnd = sURL.Length;
            string sPage = sURL.Substring(nStart, nEnd - nStart);
            return sPage;
        }

        public static string GetCallingPage()
        {
            string sURL = HttpContext.Current.Request.FilePath;

            Int32 nStart = sURL.LastIndexOf('/') + 1;
            Int32 nEnd = sURL.Length;
            string sPage = sURL.Substring(nStart, nEnd - nStart);
            return sPage;
        }
        #endregion

        public static string GetQuerystring(string url)
        {
            if (url.Contains("?"))
            {
                return url.Substring(url.IndexOf('?') + 1);
            }

            return string.Empty;

        }

        public static string GetLinkWithoutQuerystring(string url, bool isCreateAbsolute)
        {
            if (url.Contains("?"))
            {
                url = url.Substring(0, url.IndexOf('?'));
            }

            if (isCreateAbsolute)
                return MakeAbsolute(null, url);
            else
                return url;
        }

        public static string GetLinkWithoutQuerystring(string url)
        {
            return GetLinkWithoutQuerystring(url, false); ;
        }

        public static string ActualUrlDecode(string url)
        {
            //string value = url;
            //do
            //{
            //    value = HttpUtility.UrlDecode(value);
            //} while (value != HttpUtility.UrlDecode(value));

            //return value;
            return HttpUtility.UrlDecode(url);
        }

        public static string StripURL(string value)
        {
            HttpContext context = HttpContext.Current;

            value = ActualUrlDecode(value);

            Uri uri = GetActualUri();

            //logger.DebugFormat("value '{0}' actual '{1}'", value, uri.ToString());

            if (uri == null)
            {
                return string.Empty;
            }
            else
            {
                string baseURL = uri.ToString();

                string result = string.Empty;
                if (!value.StartsWith(baseURL))
                {
                    value = Regex.Replace(value, @"^(.*?://.*?)(:\d+?)([/].*)", delegate(Match m)
                    {
                        return string.Concat(m.Groups[1].Value, m.Groups[3].Value);
                    });
                }

                //logger.DebugFormat("after port handling - value '{0}' actual '{1}'", value, uri.ToString());
                if (value.StartsWith(baseURL))
                {
                    result = value.Remove(0, uri.ToString().Length);
                    return result.TrimEnd('/').Trim();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public static NameValueCollection GetNameValueCollectionFromQueryString(string queryString, string url, bool blnIgnoreQueryItemsNotInBase64)
        {
            NameValueCollection col = GetNameValueCollectionFromQueryString(queryString);

            if (blnIgnoreQueryItemsNotInBase64)
            {
                #region Handles Query Items that are not in base 64
                string lowerQuery = queryString.ToLower();
                List<string> lstKeysToRemove = new List<string>();

                List<QueryItem> lstIgnoredQueryItems = new List<QueryItem>();
                foreach (QueryItem item in QueryConfigManager.Instance.Data.Base64.BypassBase64.QueryItems.GlobalScope)
                {
                    lstIgnoredQueryItems.Add(item);
                }

                foreach (PageItem item in QueryConfigManager.Instance.Data.Base64.BypassBase64.QueryItems.PageScope)
                {
                    if (url.Contains(item.PagePath))
                    {
                        foreach (QueryItem queryItem in item)
                        {
                            lstIgnoredQueryItems.Add(queryItem);
                        }
                    }
                }

                foreach (QueryItem item in lstIgnoredQueryItems)
                {
                    string lowerKey = item.Key.ToLower();
                    if (lowerQuery.Contains(lowerKey))
                    {
                        int indexOfKey = lowerQuery.IndexOf(lowerKey);
                        int indexOfValue = indexOfKey + item.Key.Length + 1;

                        string strValue;
                        int indexOfNextAmper = lowerQuery.IndexOf("&", indexOfValue);
                        if (indexOfNextAmper != -1)
                        {
                            strValue = lowerQuery.Substring(indexOfValue, indexOfNextAmper - indexOfValue);
                            queryString = queryString.Substring(0, indexOfKey) + queryString.Substring(indexOfNextAmper + 1, lowerQuery.Length - indexOfNextAmper - 1);
                        }
                        else
                        {
                            strValue = lowerQuery.Substring(indexOfValue, lowerQuery.Length - indexOfValue);
                            if (lowerQuery.Contains("&"))
                                queryString = queryString.Substring(0, indexOfKey - 1);
                            else //the only query
                                queryString = string.Empty;
                        }

                        lowerQuery = queryString.ToLower();
                        lstKeysToRemove.Add(item.Key.ToLower());
                    }
                }

                foreach (string strToRemove in lstKeysToRemove)
                {
                    col.Remove(strToRemove);
                }
                #endregion
            }

            return col;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns>NameValueCollection - Keys are in lower case </returns>
        public static NameValueCollection GetNameValueCollectionFromQueryString(string queryString)
        {
            NameValueCollection queryParameters = new NameValueCollection();

            if (!string.IsNullOrEmpty(queryString))
            {
                if (queryString[0] == '?')
                {
                    queryString = queryString.Substring(1);
                }

                string[] querySegments = queryString.Split('&');

                foreach (string segment in querySegments)
                {
                    if (segment.Contains("="))
                    {
                        string[] parts = segment.Split('=');
                        if (parts.Length > 0)
                        {
                            string key = parts[0].Trim(new char[] { '?', ' ' });
                            string val = parts[1].Trim();
                            queryParameters.Add(key.ToLower(), val);
                        }
                    }
                    else if (Tvinci.Web.HttpModules.Configuration.QueryConfigManager.Base64Mode)
                    {
                        // Try to decode using base-64
                        try
                        {
                            string convertedFromBase64 = QueryStringHelper.DecryptQueryString(segment);

                            queryParameters.Add(GetNameValueCollectionFromQueryString(convertedFromBase64));
                        }
                        catch (Exception)
                        {
                            throw new Exception("Failed Converting:" + queryString);
                        }
                    }
                    else
                    {
                        throw new Exception("GetNameValueCollectionFromQueryString - Unknown state");
                    }
                }
            }

            return queryParameters;
        }


        /// <summary>
        /// removes the parameter from the url.
        /// </summary>
        /// <param name="param">Parameter to be removed</param>
        /// <param name="Url">URL from (not encrypted)</param>
        /// <param name="ParamValue">Optional - removes the parameter if equals to this value (not case sensitive)</param>
        /// <returns></returns>
        public static string RemoveQueryParamterFromURL(string param, string Url, string ParamValue)
        {
            string lowerParam = param.ToLower();
            string OriginalQuery = LinkHelper.GetQuerystring(Url);
            if (OriginalQuery != string.Empty)
            {
                string query = OriginalQuery.Clone() as string;
                if (Tvinci.Web.HttpModules.Configuration.QueryConfigManager.Base64Mode)
                {
                    string lowerQuery = query.ToLower();
                    //for example if Language parameter exists the value is in length 2 (i.e. "he" or "ru")

                    string DecryptedParam = string.Empty;

                    if (lowerQuery.Contains(lowerParam))
                    {
                        int index = lowerQuery.IndexOf(lowerParam);
                        int indexOfNextParam = lowerQuery.IndexOf("&", index);
                        int lengthOfParam;
                        if (indexOfNextParam == -1)
                            lengthOfParam = lowerQuery.Length - index - param.Length - 1;//1 is for '='
                        else
                            lengthOfParam = indexOfNextParam - index - 1;

                        if (lowerQuery.Substring(index + 9, lengthOfParam) == ParamValue.ToLower())
                        {
                            if (query.Length > (index + param.Length + ParamValue.Length + 1))
                            {//Param is not in the end
                                query = query.Substring(0, index) + query.Substring(index + param.Length + ParamValue.Length + 2, query.Length);
                            }
                            else
                            {
                                if (lowerQuery.Contains("&" + lowerParam))
                                {
                                    query = query.Substring(0, index - 1);
                                }
                                else
                                {
                                    query = query.Substring(0, index);
                                }
                            }
                        }
                        else
                        {
                            //if (lowerQuery.Contains("&" + lowerParam))
                            //    DecryptedParam = query.Substring(index - 1, param.Length + ParamValue.Length + 2);
                            //else
                            DecryptedParam = query.Substring(index, param.Length + ParamValue.Length + 1);

                            query = query.Replace(DecryptedParam, string.Empty);
                        }
                    }

                    if (query.Length > 0)
                    {
                        if (query[0] == '&')
                        {
                            query = QueryStringHelper.DecryptQueryString(query.Substring(1));
                            query = query + "&" + DecryptedParam;
                        }
                        else if (query[query.Length - 1] == '&')
                        {
                            query = QueryStringHelper.DecryptQueryString(query.Substring(0, query.Length - 1));
                            query = query + "&" + DecryptedParam;
                        }
                        else
                        {//there wasn't decrypted string
                            query = QueryStringHelper.DecryptQueryString(query);
                        }
                    }
                    else
                    {
                        query = DecryptedParam;
                    }
                }

                NameValueCollection col = LinkHelper.GetNameValueCollectionFromQueryString(query, Url, true);

                //removes the parameter
                if (col[param] != null)
                {
                    if (string.IsNullOrEmpty(ParamValue) || col[param].ToLower() == ParamValue.ToLower())
                    {
                        col.Remove(param);
                    }

                    for (int i = 0; i < col.Count; i++)
                    {
                        if (i == 0)
                        {
                            query = col.Keys[i] + "=" + col[i];
                        }
                        else
                        {
                            query += "&" + col.Keys[i] + "=" + col[i];
                        }
                    }
                }

                Url = Url.Replace(OriginalQuery, query);

                if (query == string.Empty)
                {//removes the '?' for the query
                    Url = Url.Substring(0, Url.Length - 1);
                }
            }

            return Url;
        }
    }
}
