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

        public ElasticSearchConfiguration(string key) : base(key)
        {
            URL = new StringConfigurationValue("url", this)
            {
                DefaultValue = "http://elasticsearch-new:9200"
            };
            URLV1 = new StringConfigurationValue("url_v1", this)
            {
                DefaultValue = "http://elasticsearch:9200"
            };
            URLV2 = new StringConfigurationValue("url_v2", this)
            {
                DefaultValue = "http://elasticsearch-new:9200"
            };
            MaxNGram = new NumericConfigurationValue("max_ngram", this)
            {
                DefaultValue = 20,
                Description = "Maximum size of search tokens for 'like' and 'autocomplete' in ElasticSearch. It should be synced with analyzers in remote tasks"
            };
            AlternativeUrl = new StringConfigurationValue("alt_url")
            {
                ShouldAllowEmpty = true,
                Description = "Alternative, backup ElasticSearch server location. In most cases we don't use this."
            };
            MaxResults = new NumericConfigurationValue("max_results", this)
            {
                DefaultValue = 100000,
                ShouldAllowEmpty = true
            };
        }
    }
}