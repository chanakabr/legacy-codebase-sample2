using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Helpers.FriendlyUrl
{
    public interface IFriendlyUrlConfiguration
    {
        Dictionary<IFriendlyUrlProvider, FriendlyUrlTranslationItem[]> ExtractProvidersAndCategoryTranslation();

        string GetDefaultUrl();

        string AppendLanguageToQueryString(string theCurrentQueryString, string theLanguage);

        bool ShouldUseCache();
    }
}
