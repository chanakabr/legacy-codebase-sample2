using ApiObjects.CanaryDeployment.Elasticsearch;
using ConfigurationManager;
using ElasticSearch.Searcher;
using ElasticSearch.Searcher.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ElasticSearch.Common
{
    public interface IElasticSearchIndexDefinitions
    {
        string GetAnalyzerDefinition(string sAnalyzerName);
        string GetFilterDefinition(string sFilterName);
        string GetTokenizerDefinition(string tokenizerName);
        bool AnalyzerExists(string sAnalyzerName);
        bool FilterExists(string sFilterName);
        bool TokenizerExists(string tokenizerName);

        List<Analyzer> GetAnalyzers(ElasticsearchVersion version, string languageCode);
        List<Tokenizer> GetTokenizers(ElasticsearchVersion version, string languageCode);
        List<Filter> GetFilters(ElasticsearchVersion version, string languageCode);

    }

    public class ElasticSearchIndexDefinitions : IElasticSearchIndexDefinitions
    {
        private static readonly Lazy<ElasticSearchIndexDefinitions> LazyInstance = new Lazy<ElasticSearchIndexDefinitions>(() =>
            new ElasticSearchIndexDefinitions(Utils.Instance, ApplicationConfiguration.Current), LazyThreadSafetyMode.PublicationOnly);
        public static ElasticSearchIndexDefinitions Instance => LazyInstance.Value;

        private readonly IElasticSearchCommonUtils _utils;
        private readonly IApplicationConfiguration _configuration;

        private static Dictionary<string, string> dESAnalyzers = new Dictionary<string, string>();
        private static Dictionary<string, string> dESFilters = new Dictionary<string, string>();
        private static Dictionary<string, string> tokenizers = new Dictionary<string, string>();

        public ElasticSearchIndexDefinitions(IElasticSearchCommonUtils utils, IApplicationConfiguration applicationConfiguration)
        {
            _utils = utils;
            _configuration = applicationConfiguration;
        }

        public string GetAnalyzerDefinition(string sAnalyzerName)
        {
            string analyzer;

            if (!dESAnalyzers.TryGetValue(sAnalyzerName, out analyzer))
            {

                analyzer = _utils.GetTcmValue(sAnalyzerName);
                if (!string.IsNullOrEmpty(analyzer))
                    dESAnalyzers[sAnalyzerName] = analyzer;
            }

            return analyzer;
        }

        public string GetFilterDefinition(string sFilterName)
        {
            string filter;

            if (!dESFilters.TryGetValue(sFilterName, out filter))
            {
                filter = _utils.GetTcmValue(sFilterName);
                if (!string.IsNullOrEmpty(filter))
                    dESFilters[sFilterName] = filter;
            }

            return filter;
        }

        public string GetTokenizerDefinition(string tokenizerName)
        {
            string tokenizer;

            if (!tokenizers.TryGetValue(tokenizerName, out tokenizer))
            {
                tokenizer = _utils.GetTcmValue(tokenizerName);

                if (!string.IsNullOrEmpty(tokenizer))
                {
                    tokenizers[tokenizerName] = tokenizer;
                }
            }

            return tokenizer;
        }

        public bool AnalyzerExists(string sAnalyzerName) => !string.IsNullOrEmpty(GetAnalyzerDefinition(sAnalyzerName));
        public bool FilterExists(string sFilterName) => !string.IsNullOrEmpty(GetFilterDefinition(sFilterName));
        public bool TokenizerExists(string tokenizerName) => !string.IsNullOrEmpty(GetTokenizerDefinition(tokenizerName));

        public List<Analyzer> GetAnalyzers(ElasticsearchVersion version, string languageCode)
        {
            List<Analyzer> result = new List<Analyzer>();
            string versionString = string.Empty;
            switch (version)
            {
                case ElasticsearchVersion.ES_7_13:
                    versionString = "7";
                    break;
                default:
                    versionString = "7";
                    break;
            }

            if (_configuration.ElasticSearchConfiguration.ShouldUseClassAnalyzerDefinitions.Value)
            {
                // TODO: future implementation
            }
            else
            {
                string tcmKey = Utils.GetLangCodeAnalyzerKey(languageCode, versionString);
                string analyzerString = GetAnalyzerDefinition(tcmKey);
                string jsonAnalyzerString = $"{{analyzerString}}";
                var analyzerDefinitions = JsonConvert.DeserializeObject<AnalyzerDefinitions>(jsonAnalyzerString);

                result = analyzerDefinitions.Analyzers.Values.ToList();
            }

            return result;
        }

        public List<Tokenizer> GetTokenizers(ElasticsearchVersion version, string languageCode)
        {
            throw new NotImplementedException();
        }

        public List<Filter> GetFilters(ElasticsearchVersion version, string languageCode)
        {
            throw new NotImplementedException();
        }
    }
}
