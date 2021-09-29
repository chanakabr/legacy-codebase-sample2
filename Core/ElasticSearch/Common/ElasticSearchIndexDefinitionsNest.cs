using System;
using System.Collections.Generic;
using System.Threading;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ConfigurationManager;
using ElasticSearch.Searcher.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElasticSearch.Common
{
    public interface IElasticSearchIndexDefinitionsNest:IElasticSearchIndexDefinitions
    {
        Dictionary<string, Analyzer> GetAnalyzers(ElasticsearchVersion version, string languageCode);
        Dictionary<string, Tokenizer> GetTokenizers(ElasticsearchVersion version, string languageCode);
        Dictionary<string, Filter> GetFilters(ElasticsearchVersion version, string languageCode);
    }
    
    public class ElasticSearchIndexDefinitionsNest :ElasticSearchIndexDefinitions, IElasticSearchIndexDefinitionsNest
    {
        private static readonly Lazy<ElasticSearchIndexDefinitionsNest> LazyInstance = new Lazy<ElasticSearchIndexDefinitionsNest>(() =>
            new ElasticSearchIndexDefinitionsNest(Common.Utils.Instance, ApplicationConfiguration.Current), LazyThreadSafetyMode.PublicationOnly);
        public static ElasticSearchIndexDefinitionsNest DefinitionInstance => LazyInstance.Value;
        
        public ElasticSearchIndexDefinitionsNest(IElasticSearchCommonUtils utils, IApplicationConfiguration applicationConfiguration)
            :base(utils,applicationConfiguration){}
        
        public Dictionary<string, Analyzer> GetAnalyzers(ElasticsearchVersion version, string languageCode)
        {
            var result = new Dictionary<string, Analyzer>();
            string versionString = string.Empty;
            switch (version)
            {
                case ElasticsearchVersion.ES_7:
                    versionString = "7";
                    break;
                default:
                    versionString = "7";
                    break;
            }

            string tcmKey = Common.Utils.GetLangCodeAnalyzerKey(languageCode, versionString);
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
                case ElasticsearchVersion.ES_7:
                    versionString = "7";
                    break;
                default:
                    versionString = "7";
                    break;
            }


            string tcmKey = Common.Utils.GetLangCodeFilterKey(languageCode, versionString);
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
                case ElasticsearchVersion.ES_7:
                    versionString = "7";
                    break;
                default:
                    versionString = "7";
                    break;
            }

            string tcmKey = Common.Utils.GetLangCodeFilterKey(languageCode, versionString);
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