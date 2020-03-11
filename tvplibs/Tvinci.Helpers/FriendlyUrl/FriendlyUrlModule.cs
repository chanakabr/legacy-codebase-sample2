using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Helpers.FriendlyUrl;

namespace Tvinci.Helpers.FriendlyUrl
{
    public class FriendlyUrlModule : IHttpModule
    {
        #region IHttpModule Members
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
        }
        #endregion

        #region Event Handlers
        void context_BeginRequest(object sender, EventArgs e)
        {
            string url = HttpContext.Current.Request.Url.AbsolutePath;

            if (url.Contains("404.aspx"))
            {
                string[] urlInfo404 = HttpContext.Current.Request.Url.Query.ToString().Split(';');
                if (urlInfo404.Length > 1)
                {
                    string originalUrl = urlInfo404[1];
                    string[] urlParts = originalUrl.Split('?');

                    string queryString = string.Empty;
                    string requestedFile = string.Empty;

                    if (urlParts.Length > 1)
                    {
                        requestedFile = urlParts[0];
                        queryString = urlParts[1];
                    }
                    else
                    {
                        requestedFile = urlParts[0];
                    }

                    if (requestedFile.IndexOf('.') > 0)
                    {
                        // There's some extension, so this is not an extensionless URL.
                        // Don't handle such URL because these are really missing files
                    }
                    else
                    {
                        // Extensionless URL. Use your URL rewriting logic to handle such URL
                        HandleUrl(requestedFile, queryString);
                    }
                }
            }
        }
        #endregion

        #region Fields
        private static bool itsInitialized;
        private static Dictionary<string, FriendlyUrlProviderLanguageIDPair> itsDictionary;
        private static Dictionary<string, FriendlyUrlHandlerCacheObject> itsCache;
        private static IFriendlyUrlConfiguration itsConfiguration;
        private static List<IFriendlyUrlProvider> itsProviders;
        private static bool itsShouldUseCache;
        private static DateTime LastCachcResetTimeStamp = DateTime.Now;
        #endregion

        #region Properties
        public static IFriendlyUrlConfiguration Configuration
        {
            get
            {
                return itsConfiguration;
            }
            set
            {
                itsConfiguration = value;
                InitModule();
            }
        }
        #endregion

        #region Private Methods
        private static void InitModule()
        {
            if (Configuration == null)
                return;

            try
            {
                Dictionary<IFriendlyUrlProvider, FriendlyUrlTranslationItem[]> providers = Configuration.ExtractProvidersAndCategoryTranslation();

                // Reverse given dictionary and add providers - so for each translation we get the matching provider
                itsDictionary = new Dictionary<string, FriendlyUrlProviderLanguageIDPair>();
                itsProviders = new List<IFriendlyUrlProvider>();
                foreach (KeyValuePair<IFriendlyUrlProvider, FriendlyUrlTranslationItem[]> entry in providers)
                {
                    for (int i = 0; i < entry.Value.Length; i++)
                    {
                        if (itsProviders.Contains(entry.Key))
                        {
                            throw new Exception(string.Format("Translation: {0}, already exists in the configuration. Cannot have more than one provider for the same translation", entry.Value[i].Translation));
                        }

                        itsDictionary.Add(entry.Value[i].Translation,
                            new FriendlyUrlProviderLanguageIDPair(entry.Key, entry.Value[i].LanguageID));
                    }

                    itsProviders.Add(entry.Key);
                }

                // Set cache mode
                itsCache = new Dictionary<string, FriendlyUrlHandlerCacheObject>();
                itsShouldUseCache = Configuration.ShouldUseCache();

                itsInitialized = true;
            }
            catch
            {
                itsInitialized = false;
            }
        }

        private static void ProcessRequest(HttpContext context, string theUrl, string theQueryString)
        {
            // Split request path into parts
            string relRequest = theUrl.Substring(theUrl.IndexOf(context.Request.ApplicationPath) + context.Request.ApplicationPath.Length);
            if (string.IsNullOrEmpty(relRequest))
            {
                RewritePath(context, theUrl, itsConfiguration.GetDefaultUrl(), string.Empty);
                return;
            }

            if (relRequest[0] == '/')
                relRequest = relRequest.Substring(1);

            string[] requestParts = relRequest.Split('/');

            if (requestParts.Length == 0)
            {
                RewritePath(context, theUrl, itsConfiguration.GetDefaultUrl(), string.Empty);
                return;
            }

            // Check if a provider is given for the category
            string category = requestParts[0];

            FriendlyUrlProviderLanguageIDPair provLangPair;
            itsDictionary.TryGetValue(category.ToLower(), out provLangPair);

            if (provLangPair == null)
            {
                // No provider - return default page
                RewritePath(context, theUrl, itsConfiguration.GetDefaultUrl(), string.Empty);
            }
            else
            {
                // Run provider on request
                string overridenLanguage;
                string newQueryString;

                string newUrl = provLangPair.Provider.HandleRequest(category, provLangPair.LanguageID,
                    requestParts, out newQueryString, out overridenLanguage);

                // Check if url is empty
                if (string.IsNullOrEmpty(newUrl))
                {
                    RewritePath(context, theUrl, itsConfiguration.GetDefaultUrl(), string.Empty);
                    return;
                }

                // Add old query string


                // Add language
                string lang = string.IsNullOrEmpty(overridenLanguage) ? provLangPair.LanguageID : overridenLanguage;
                newQueryString = itsConfiguration.AppendLanguageToQueryString(newQueryString, lang);

                RewritePath(context, theUrl, newUrl, newQueryString);
            }
        }

        private static void RewritePath(HttpContext context, string theOrigURL, string theURL, string theQueryString)
        {
            if (itsShouldUseCache)
            {
                itsCache.Add(theOrigURL, new FriendlyUrlHandlerCacheObject(theURL, theQueryString));
            }

            context.RewritePath(theURL, string.Empty, theQueryString, false);
        }

        private void HandleUrl(string theUrl, string theQueryString)
        {
            // If not initialized - return default page
            if (!itsInitialized)
            {
                throw new Exception("Friendly url handler not initialized");
            }

            if (itsShouldUseCache)
            {
                // Check if request is in cache
                FriendlyUrlHandlerCacheObject obj;
                if (itsCache.TryGetValue(theUrl, out obj))
                    HttpContext.Current.RewritePath(obj.URL, string.Empty, obj.QueryString, false);
                else
                    ProcessRequest(HttpContext.Current, theUrl, theQueryString);

                // Check if cache should be cleared
                TimeSpan elapsed = DateTime.Now.Subtract(LastCachcResetTimeStamp);
                if (elapsed.Hours >= 1)
                {
                    // Every 12 hours - clear cache
                    itsCache = new Dictionary<string, FriendlyUrlHandlerCacheObject>();
                    LastCachcResetTimeStamp = DateTime.Now;
                }
            }
            else
                ProcessRequest(HttpContext.Current, theUrl, theQueryString);
        }

        #endregion

        #region Public Methods
        public static string GetFriendlyUrl(IFriendlyUrlParameter theParameter)
        {
            if (itsProviders == null)
                return itsConfiguration.GetDefaultUrl();

            for (int i = 0; i < itsProviders.Count; i++)
            {
                if (itsProviders[i].ProviderID.Equals(theParameter.ProviderID))
                    return itsProviders[i].CreateFriendlyUrl(theParameter);
            }

            // No provider found - return default url
            return itsConfiguration.GetDefaultUrl();
        }

        public static void ClearCache()
        {
            itsCache = new Dictionary<string, FriendlyUrlHandlerCacheObject>();
        }
        #endregion
    }
}