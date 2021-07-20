using System;
using System.Collections.Generic;
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
    }

    public class ElasticSearchIndexDefinitions : IElasticSearchIndexDefinitions
    {
        private static readonly Lazy<ElasticSearchIndexDefinitions> LazyInstance = new Lazy<ElasticSearchIndexDefinitions>(() =>
            new ElasticSearchIndexDefinitions(Utils.Instance), LazyThreadSafetyMode.PublicationOnly);
        public static ElasticSearchIndexDefinitions Instance => LazyInstance.Value;

        private readonly IElasticSearchCommonUtils _utils;

        private static Dictionary<string, string> dESAnalyzers = new Dictionary<string, string>();
        private static Dictionary<string, string> dESFilters = new Dictionary<string, string>();
        private static Dictionary<string, string> tokenizers = new Dictionary<string, string>();

        public ElasticSearchIndexDefinitions(IElasticSearchCommonUtils utils)
        {
            _utils = utils;
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
    }
}
