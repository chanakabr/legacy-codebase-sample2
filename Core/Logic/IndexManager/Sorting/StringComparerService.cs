using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace ApiLogic.IndexManager.Sorting
{
    public class StringComparerService : IStringComparerService
    {
        private static readonly IDictionary<string, string> MappedLanguageCodes = new Dictionary<string, string>()
        {
            { "frn", "fra" },
            { "mnd", "chi" },
            { "hnd", "hin" },
            { "csp", "spa" },
            { "cze", "ces" }
        };

        private static readonly ConcurrentDictionary<string, IComparer<string>> Comparers = new ConcurrentDictionary<string, IComparer<string>>();

        private static readonly Lazy<IStringComparerService> LazyInstance = new Lazy<IStringComparerService>(() => new StringComparerService(), LazyThreadSafetyMode.PublicationOnly);

        public static IStringComparerService Instance => LazyInstance.Value;

        public IComparer<string> GetComparer(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                return null;
            }

            if (!MappedLanguageCodes.TryGetValue(languageCode, out var mappedLanguageCode))
            {
                mappedLanguageCode = languageCode;
            }

            var comparer = Comparers.GetOrAdd(mappedLanguageCode, language => StringComparer.Create(new CultureInfo(language), false));
            
            return comparer;
        }
    }
}