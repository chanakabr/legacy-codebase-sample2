using System;
using System.Threading;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ElasticSearch.Common;

namespace ElasticSearch.Utils
{
    public class SpecialSortingServiceV7 : SpecialSortingServiceBase
    {
        private static readonly Lazy<ISpecialSortingService> LazyInstance = new Lazy<ISpecialSortingService>(() => new SpecialSortingServiceV7(ElasticSearchIndexDefinitionsNest.DefinitionInstance), LazyThreadSafetyMode.PublicationOnly);

        private readonly IElasticSearchIndexDefinitionsNest _indexDefinitions;

        public static ISpecialSortingService Instance => LazyInstance.Value;

        public SpecialSortingServiceV7(IElasticSearchIndexDefinitionsNest indexDefinitions)
        {
            _indexDefinitions = indexDefinitions;
        }

        protected override bool IsSpecialSortingLanguage(string languageCode)
        {
            var customProperties = _indexDefinitions.GetCustomProperties(ElasticsearchVersion.ES_7, languageCode);

            return customProperties.ContainsKey($"{languageCode}_sort");
        }

        protected override string GetLanguageKey(string languageCode)
        {
            return $"{languageCode}_v7";
        }
    }
}