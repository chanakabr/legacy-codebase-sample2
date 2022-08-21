using Phx.Lib.Appconfig;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ElasticSearch.Common
{
    public interface IElasticSearchIndexDefinitionsBase
    {
        string GetAnalyzerDefinition(string sAnalyzerName);
        string GetCustomPropertiesDefinition(string customPropertiesKey);
        string GetFilterDefinition(string sFilterName);
        string GetTokenizerDefinition(string tokenizerName);
    }
    
    public interface IElasticSearchIndexDefinitions:IElasticSearchIndexDefinitionsBase
    {
        bool AnalyzerExists(string analyzerName);
        bool FilterExists(string filterName);
        bool TokenizerExists(string tokenizerName);
    }

    

    public class ElasticSearchIndexDefinitions : IElasticSearchIndexDefinitions
    {
        private static readonly Lazy<ElasticSearchIndexDefinitions> LazyInstance = new Lazy<ElasticSearchIndexDefinitions>(() =>
            new ElasticSearchIndexDefinitions(Common.Utils.Instance, ApplicationConfiguration.Current), LazyThreadSafetyMode.PublicationOnly);
        public static ElasticSearchIndexDefinitions Instance => LazyInstance.Value;

        internal readonly IElasticSearchCommonUtils Utils;
        internal readonly IApplicationConfiguration Configuration;

        private static ConcurrentDictionary<string, string> dESAnalyzers = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, string> dESCustomProperties = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, string> dESFilters = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, string> tokenizers = new ConcurrentDictionary<string, string>();

        public ElasticSearchIndexDefinitions(IElasticSearchCommonUtils utils, IApplicationConfiguration applicationConfiguration)
        {
            Utils = utils;
            Configuration = applicationConfiguration;
        }

        public string GetAnalyzerDefinition(string analyzerName)
        {
            if (!dESAnalyzers.TryGetValue(analyzerName, out string analyzer))
            {
                analyzer = Utils.GetTcmValue(analyzerName);
                if (!string.IsNullOrEmpty(analyzer))
                    dESAnalyzers.TryAdd(analyzerName, analyzer);
            }
            return analyzer;
        }

        public string GetCustomPropertiesDefinition(string customPropertiesKey)
        {
            if (!dESCustomProperties.TryGetValue(customPropertiesKey, out var customPropertiesDefinition))
            {
                customPropertiesDefinition = Utils.GetTcmValue(customPropertiesKey);
                if (!string.IsNullOrEmpty(customPropertiesDefinition))
                {
                    dESCustomProperties.TryAdd(customPropertiesKey, customPropertiesDefinition);
                }
            }

            return customPropertiesDefinition;
        }

        public string GetFilterDefinition(string filterName)
        {
            if (!dESFilters.TryGetValue(filterName, out string filter))
            {
                filter = Utils.GetTcmValue(filterName);
                if (!string.IsNullOrEmpty(filter))
                    dESFilters.TryAdd(filterName, filter);
            }
            return filter;
        }

        public string GetTokenizerDefinition(string tokenizerName)
        {
            if (!tokenizers.TryGetValue(tokenizerName, out string tokenizer))
            {
                tokenizer = Utils.GetTcmValue(tokenizerName);
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
    }
}
