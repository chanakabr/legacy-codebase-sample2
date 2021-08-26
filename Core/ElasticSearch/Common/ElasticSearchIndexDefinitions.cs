using ApiObjects.CanaryDeployment.Elasticsearch;
using ConfigurationManager;
using ElasticSearch.Searcher.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ElasticSearch.Common
{
    public interface IElasticSearchIndexDefinitions
    {
        string GetAnalyzerDefinition(string sAnalyzerName);
        string GetFilterDefinition(string sFilterName);
        string GetTokenizerDefinition(string tokenizerName);
        bool AnalyzerExists(string analyzerName);
        bool FilterExists(string filterName);
        bool TokenizerExists(string tokenizerName);

        Dictionary<string, Analyzer> GetAnalyzers(ElasticsearchVersion version, string languageCode);
        Dictionary<string, Tokenizer> GetTokenizers(ElasticsearchVersion version, string languageCode);
        Dictionary<string, Filter> GetFilters(ElasticsearchVersion version, string languageCode);

    }

    public class ElasticSearchIndexDefinitions : IElasticSearchIndexDefinitions
    {
        private static readonly Lazy<ElasticSearchIndexDefinitions> LazyInstance = new Lazy<ElasticSearchIndexDefinitions>(() =>
            new ElasticSearchIndexDefinitions(Utils.Instance, ApplicationConfiguration.Current), LazyThreadSafetyMode.PublicationOnly);
        public static ElasticSearchIndexDefinitions Instance => LazyInstance.Value;

        private readonly IElasticSearchCommonUtils _utils;
        private readonly IApplicationConfiguration _configuration;

        private static ConcurrentDictionary<string, string> dESAnalyzers = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, string> dESFilters = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, string> tokenizers = new ConcurrentDictionary<string, string>();

        public ElasticSearchIndexDefinitions(IElasticSearchCommonUtils utils, IApplicationConfiguration applicationConfiguration)
        {
            _utils = utils;
            _configuration = applicationConfiguration;
        }

        public string GetAnalyzerDefinition(string analyzerName)
        {
            if (!dESAnalyzers.TryGetValue(analyzerName, out string analyzer))
            {
                analyzer = _utils.GetTcmValue(analyzerName);
                if (!string.IsNullOrEmpty(analyzer))
                    dESAnalyzers.TryAdd(analyzerName, analyzer);
            }
            return analyzer;
        }

        public string GetFilterDefinition(string filterName)
        {
            if (!dESFilters.TryGetValue(filterName, out string filter))
            {
                filter = _utils.GetTcmValue(filterName);
                if (!string.IsNullOrEmpty(filter))
                    dESFilters.TryAdd(filterName, filter);
            }
            return filter;
        }

        public string GetTokenizerDefinition(string tokenizerName)
        {
            if (!tokenizers.TryGetValue(tokenizerName, out string tokenizer))
            {
                tokenizer = _utils.GetTcmValue(tokenizerName);
                if (!string.IsNullOrEmpty(tokenizer))
                {
                    tokenizers.TryAdd(tokenizerName, tokenizer);
                }
            }
            return tokenizer;
        }

        public bool AnalyzerExists(string sAnalyzerName) => !string.IsNullOrEmpty(GetAnalyzerDefinition(sAnalyzerName));
        public bool FilterExists(string sFilterName) => !string.IsNullOrEmpty(GetFilterDefinition(sFilterName));
        public bool TokenizerExists(string tokenizerName) => !string.IsNullOrEmpty(GetTokenizerDefinition(tokenizerName));

        public Dictionary<string, Analyzer> GetAnalyzers(ElasticsearchVersion version, string languageCode)
        {
            var result = new Dictionary<string, Analyzer>();
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

            string tcmKey = Utils.GetLangCodeAnalyzerKey(languageCode, versionString);
            string analyzerString = GetAnalyzerDefinition(tcmKey);

            if (string.IsNullOrEmpty(analyzerString))
            {
                return result;
            }

            string jsonAnalyzerString = $"{{{analyzerString}}}";
            result = JsonConvert.DeserializeObject<Dictionary<string, Analyzer>>(jsonAnalyzerString);

            return result;
        }

        public Dictionary<string, Filter> GetFilters(ElasticsearchVersion version, string languageCode)
        {
            var result = new Dictionary<string, Filter>();
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


            string tcmKey = Utils.GetLangCodeFilterKey(languageCode, versionString);
            string filterString = GetFilterDefinition(tcmKey);

            if (string.IsNullOrEmpty(filterString))
            {
                return result;
            }

            string jsonFilterString = $"{{{filterString}}}";
            JObject jObject = JObject.Parse(jsonFilterString);

            foreach (var jsonFilter in jObject)
            {
                string type = jsonFilter.Value.Value<string>("type");

                switch (type.ToLower())
                {
                    case "ngram":
                    case "edgengram":
                        {
                            var parsedFilter = jsonFilter.Value.ToObject<NgramFilter>();
                            result.Add(jsonFilter.Key, parsedFilter);
                            break;
                        }
                    default:
                        break;
                }
            }

            return result;
        }

        public Dictionary<string, Tokenizer> GetTokenizers(ElasticsearchVersion version, string languageCode)
        {
            var result = new Dictionary<string, Tokenizer>();
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

            string tcmKey = Utils.GetLangCodeFilterKey(languageCode, versionString);
            string tokenizerString = GetTokenizerDefinition(tcmKey);

            if (string.IsNullOrEmpty(tokenizerString))
            {
                return result;
            }

            string jsonTokenizerString = $"{{{tokenizerString}}}";
            JObject jObject = JObject.Parse(jsonTokenizerString);

            foreach (var jsonTokenizer in jObject)
            {
                string type = jsonTokenizer.Value.Value<string>("type");

                switch (type.ToLower())
                {
                    case "kuromoji_tokenizer":
                        {
                            var parsedObject = jsonTokenizer.Value.ToObject<KuromojiTokenizer>();
                            result.Add(jsonTokenizer.Key, parsedObject);
                            break;
                        }
                    default:
                        break;
                }
            }

            return result;
        }
    }
}
