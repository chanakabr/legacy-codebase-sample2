using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.IO;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using Tvinci.Data.DataLoader.PredefinedAdapters;
using Tvinci.Helpers;
using System.Text;
using System.Reflection;

namespace Tvinci.Helpers.FriendlyUrl
{
    public class FriendlyUrlHandler : IHttpHandlerFactory
    {
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

        private static void ProcessRequest(HttpContext context, string theUrl)
        {
            // Split request path into parts
            string relRequest = theUrl.Substring(2);
            if (string.IsNullOrEmpty(relRequest))
            {
                RewritePath(context, theUrl, itsConfiguration.GetDefaultUrl(), string.Empty);
                return;
            }

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

        #region IHttpHandlerFactory Members
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            // If not initialized - return default page
            if (!itsInitialized)
            {
                throw new Exception("Friendly url handler not initialized");
            }

            string newUrl = context.Request.AppRelativeCurrentExecutionFilePath.Replace(".spec", "");

            if (itsShouldUseCache)
            {
                // Check if request is in cache
                FriendlyUrlHandlerCacheObject obj;
                if (itsCache.TryGetValue(newUrl, out obj))
                    context.RewritePath(obj.URL, string.Empty, obj.QueryString, false);
                else
                    ProcessRequest(context, newUrl);

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
                ProcessRequest(context, newUrl);

            // Return handler
            return PageParser.GetCompiledPageInstance(context.Request.Path, context.Request.PhysicalPath, context);
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
            // Does nothing
        }
        #endregion
    }

    #region InnerClasses
    #region FriendlyUrlHandlerCacheObject
    public class FriendlyUrlHandlerCacheObject
    {
        public FriendlyUrlHandlerCacheObject(string theURL, string theQueryString)
        {
            URL = theURL;
            QueryString = theQueryString;
        }

        public string URL { get; set; }
        public string QueryString { get; set; }
    }
    #endregion

    #region FriendlyUrlProviderLanguageIDPair
    public class FriendlyUrlProviderLanguageIDPair
    {
        public FriendlyUrlProviderLanguageIDPair(IFriendlyUrlProvider theProvider, string theLanguageID)
        {
            Provider = theProvider;
            LanguageID = theLanguageID;
        }

        public IFriendlyUrlProvider Provider;
        public string LanguageID;
    }
    #endregion

    #region FriendlyUrlTranslationItem
    public class FriendlyUrlTranslationItem
    {
        public FriendlyUrlTranslationItem(string theTranslation, string theLanguageID)
        {
            Translation = theTranslation;
            LanguageID = theLanguageID;
        }

        public string Translation;
        public string LanguageID;
    }
    #endregion
    #endregion
}