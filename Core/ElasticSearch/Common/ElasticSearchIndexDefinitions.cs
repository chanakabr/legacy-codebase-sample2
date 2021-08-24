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
    }

    public class ElasticSearchIndexDefinitions : IElasticSearchIndexDefinitions
    {
        private static readonly Lazy<ElasticSearchIndexDefinitions> LazyInstance = new Lazy<ElasticSearchIndexDefinitions>(() =>
            new ElasticSearchIndexDefinitions(Utils.Instance), LazyThreadSafetyMode.PublicationOnly);
        public static ElasticSearchIndexDefinitions Instance => LazyInstance.Value;

        private readonly IElasticSearchCommonUtils _utils;

        private static ConcurrentDictionary<string, string> dESAnalyzers = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, string> dESFilters = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, string> tokenizers = new ConcurrentDictionary<string, string>();

        public ElasticSearchIndexDefinitions(IElasticSearchCommonUtils utils)
        {
            _utils = utils;
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
    }
}
