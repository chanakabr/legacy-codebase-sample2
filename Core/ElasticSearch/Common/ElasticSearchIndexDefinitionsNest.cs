using System;
using System.Collections.Generic;
using System.Threading;
using ApiObjects.CanaryDeployment.Elasticsearch;
using Phx.Lib.Appconfig;
using ElasticSearch.Searcher.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElasticSearch.Common
{
    public interface IElasticSearchIndexDefinitionsNest : IElasticSearchIndexDefinitions
    {
        Dictionary<string, Analyzer> GetAnalyzers(ElasticsearchVersion version, string languageCode);
        Dictionary<string, CustomProperty> GetCustomProperties(ElasticsearchVersion version, string languageCode);
        Dictionary<string, Tokenizer> GetTokenizers(ElasticsearchVersion version, string languageCode);
        Dictionary<string, Filter> GetFilters(ElasticsearchVersion version, string languageCode);
    }

    public class ElasticSearchIndexDefinitionsNest : ElasticSearchIndexDefinitions, IElasticSearchIndexDefinitionsNest
    {
        private static readonly Lazy<ElasticSearchIndexDefinitionsNest> LazyInstance = new Lazy<ElasticSearchIndexDefinitionsNest>(() =>
            new ElasticSearchIndexDefinitionsNest(Common.Utils.Instance, ApplicationConfiguration.Current), LazyThreadSafetyMode.PublicationOnly);
        public static ElasticSearchIndexDefinitionsNest DefinitionInstance => LazyInstance.Value;
        
        public ElasticSearchIndexDefinitionsNest(IElasticSearchCommonUtils utils, IApplicationConfiguration applicationConfiguration)
            :base(utils,applicationConfiguration){}
        
        public Dictionary<string, Analyzer> GetAnalyzers(ElasticsearchVersion version, string languageCode)
        {
            var result = new Dictionary<string, Analyzer>();

            string versionString = GetVersionString(version);
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

        public Dictionary<string, CustomProperty> GetCustomProperties(ElasticsearchVersion version, string languageCode)
        {
            var result = new Dictionary<string, CustomProperty>();

            var versionString = GetVersionString(version);
            var tcmKey = Common.Utils.GetLangCodePropertiesKey(languageCode, versionString);
            var customFieldsDefinition = GetCustomPropertiesDefinition(tcmKey);

            if (string.IsNullOrEmpty(customFieldsDefinition))
            {
                return result;
            }

            var jObject = JObject.Parse($"{{{customFieldsDefinition}}}");
            foreach (var jsonCustomField in jObject)
            {
                var type = jsonCustomField.Value?.Value<string>("type")?.ToLower();
                switch (type)
                {
                    case "icu_collation_keyword":
                    {
                        var sortProperty = jsonCustomField.Value.ToObject<SortProperty>();
                        result.Add(jsonCustomField.Key, sortProperty);
                        break;
                    }
                }
            }

            return result;
        }

        public Dictionary<string, Filter> GetFilters(ElasticsearchVersion version, string languageCode)
        {
            var result = new Dictionary<string, Filter>();

            string versionString = GetVersionString(version);
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
                        var nGramFilter = jsonFilter.Value.ToObject<NgramFilter>();
                        result.Add(jsonFilter.Key, nGramFilter);
                        break;
                    }
                    case "stemmer":
                        var stemmerFilter = jsonFilter.Value.ToObject<StemmerFilter>();
                        result.Add(jsonFilter.Key, stemmerFilter);
                        break;
                    case "phonetic":
                        var phoneticFilter = jsonFilter.Value.ToObject<PhoneticFilter>();
                        result.Add(jsonFilter.Key, phoneticFilter);
                        break;
                    case "elision":
                        var elisionFilter = jsonFilter.Value.ToObject<ElisionFilter>();
                        result.Add(jsonFilter.Key, elisionFilter);
                        break;
                }
            }

            return result;
        }

        public Dictionary<string, Tokenizer> GetTokenizers(ElasticsearchVersion version, string languageCode)
        {
            var result = new Dictionary<string, Tokenizer>();

            string versionString = GetVersionString(version);
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
                }
            }

            return result;
        }

        private string GetVersionString(ElasticsearchVersion version)
        {
            string versionString;
            switch (version)
            {
                case ElasticsearchVersion.ES_7:
                    versionString = "7";
                    break;
                default:
                    versionString = "7";
                    break;
            }

            return versionString;
        }
    }
}