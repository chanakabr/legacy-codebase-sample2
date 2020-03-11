using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class ElasticSearchConfiguration : ConfigurationValue
    {
        public StringConfigurationValue URL;
        public StringConfigurationValue URLV1;
        public StringConfigurationValue URLV2;
        public StringConfigurationValue AlternativeUrl;
        public NumericConfigurationValue MaxNGram;
        public NumericConfigurationValue MaxResults;
        public NumericConfigurationValue MaxStatSortResults;
        public NumericConfigurationValue StatSortBulkSize;
        public NumericConfigurationValue MediaPageSize;

        public ElasticSearchConfiguration(string key) : base(key)
        {
            URL = new StringConfigurationValue("url", this)
            {
                DefaultValue = "http://elasticsearch_storm:9200",
                OriginalKey = "ES_URL",
            };
            URLV1 = new StringConfigurationValue("url_v1", this)
            {
                DefaultValue = "",
                OriginalKey = "ES_URL_V1",
                ShouldAllowEmpty = true
            };
            URLV2 = new StringConfigurationValue("url_v2", this)
            {
                DefaultValue = "http://elasticsearch_storm:9200",
                OriginalKey = "ES_URL_V2",
            };
            MaxNGram = new NumericConfigurationValue("max_ngram", this)
            {
                DefaultValue = 20,
                Description = "Maximum size of search tokens for 'like' and 'autocomplete' in ElasticSearch. It should be synced with analyzers in remote tasks",
                OriginalKey = "max_ngram",
            };
            AlternativeUrl = new StringConfigurationValue("alt_url")
            {
                ShouldAllowEmpty = true,
                Description = "Alternative, backup ElasticSearch server location. In most cases we don't use this.",
                OriginalKey = "alt_url",
            };
            MaxResults = new NumericConfigurationValue("max_results", this)
            {
                DefaultValue = 100000,
                OriginalKey = "MAX_RESULTS",
                ShouldAllowEmpty = true
            };
            MaxStatSortResults = new NumericConfigurationValue("max_stat_sort_results", this)
            {
                DefaultValue = 0,
                ShouldAllowEmpty = true
            };
            StatSortBulkSize = new NumericConfigurationValue("stat_sort_bulk_size", this)
            {
                DefaultValue = 5000,
                ShouldAllowEmpty = true
            };
            MediaPageSize = new NumericConfigurationValue("media_page_size", this)
            {
                DefaultValue = 10000,
                ShouldAllowEmpty = true
            };
        }
    }
}