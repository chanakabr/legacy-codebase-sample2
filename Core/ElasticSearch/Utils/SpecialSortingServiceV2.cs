using System;
using System.Threading;
using ElasticSearch.Common;
using Newtonsoft.Json.Linq;

namespace ElasticSearch.Utils
{
    public class SpecialSortingServiceV2 : SpecialSortingServiceBase
    {
        private static readonly Lazy<ISpecialSortingService> LazyInstance = new Lazy<ISpecialSortingService>(() => new SpecialSortingServiceV2(ElasticSearchIndexDefinitions.Instance), LazyThreadSafetyMode.PublicationOnly);

        private readonly IElasticSearchIndexDefinitionsBase _indexDefinitions;

        public static ISpecialSortingService Instance => LazyInstance.Value;

        public SpecialSortingServiceV2(IElasticSearchIndexDefinitionsBase indexDefinitions)
        {
            _indexDefinitions = indexDefinitions;
        }

        protected override bool IsSpecialSortingLanguage(string languageCode)
        {
            var analyzersTcmKey = Common.Utils.GetLangCodeAnalyzerKey(languageCode, "2");
            var analyzersDefinition = _indexDefinitions.GetAnalyzerDefinition(analyzersTcmKey);
            if (string.IsNullOrEmpty(analyzersDefinition))
            {
                return false;
            }

            var jObject = JObject.Parse($"{{{analyzersDefinition}}}");

            return jObject.ContainsKey($"{languageCode}_sorting_analyzer");
        }

        protected override string GetLanguageKey(string languageCode)
        {
            return $"{languageCode}_v2";
        }
    }
}