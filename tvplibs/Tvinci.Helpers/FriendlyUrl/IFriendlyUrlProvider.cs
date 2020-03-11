using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Helpers.FriendlyUrl
{
    public interface IFriendlyUrlProvider
    {
        string ProviderID { get; }
        
        string HandleRequest(string theCategory, string theLanguage, string[] theRequestParameters, out string theQueryString, out string theOverrideLangauge);

        string CreateFriendlyUrl(IFriendlyUrlParameter theParameter);
    }

    public class StaticFriendlyUrlProvider : IFriendlyUrlProvider
    {
        private string itsUrl;

        public StaticFriendlyUrlProvider(string theUrl)
        {
            itsUrl = theUrl;
        }

        #region IFriendlyUrlProvider Members
        public string ProviderID
        {
            get { return itsUrl; }
        }

        public string HandleRequest(string theCategory, string theLanguage, string[] theRequestParameters, out string theQueryString, out string theOverrideLangauge)
        {
            theQueryString = string.Empty;
            theOverrideLangauge = string.Empty;
            return itsUrl;
        }

        public string CreateFriendlyUrl(IFriendlyUrlParameter theParameter)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    public class CategorizedFriendlyUrlProvider<T> : IFriendlyUrlProvider where T : struct
    {
        private string itsBaseUrl;
        private string itsQueryCategoryName;

        static CategorizedFriendlyUrlProvider()
        {
            if (!typeof(T).IsEnum) 
                throw new ArgumentException("T must be an enumerated type");
        }

        public CategorizedFriendlyUrlProvider(string theBaseUrl, string theQueryCategoryName)
        {
            itsBaseUrl = theBaseUrl;
            itsQueryCategoryName = theQueryCategoryName;
        }

        #region IFriendlyUrlProvider Members
        public string ProviderID
        {
            get { return itsBaseUrl; }
        }

        public string HandleRequest(string theCategory, string theLanguage, string[] theRequestParameters, out string theQueryString, out string theOverrideLangauge)
        {
            theQueryString = null;
            theOverrideLangauge = null;

            if (theRequestParameters.Length == 0)
                return string.Empty;

            // Check if a category is given and try to parse the category to the enum given
            string category = string.Empty;
            if (theRequestParameters.Length > 1 && Enum.IsDefined(typeof(T), theRequestParameters[1]))
            {
                category = theRequestParameters[1];
            }

            if (!string.IsNullOrEmpty(category))
                theQueryString = string.Format("{0}={1}", itsQueryCategoryName, category);

            return itsBaseUrl;

        }

        public string CreateFriendlyUrl(IFriendlyUrlParameter theParameter)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
