using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using System;

namespace ConfigurationManager
{
    public class ElasticSearchConfiguration : BaseConfig<ElasticSearchConfiguration>
    {
        public BaseValue<string> URL = new BaseValue<string>("url", "http://elasticsearch_storm:9200");
        public BaseValue<string> URLV2 = new BaseValue<string>("url_v2", "http://elasticsearch_storm:9200");
        public BaseValue<string> AlternativeUrl = new BaseValue<string>("alt_url", null, false, "Alternative, backup ElasticSearch server location. In most cases we don't use this.");
        public BaseValue<int> MaxNGram = new BaseValue<int>("max_ngram", 20, false, "Maximum size of search tokens for 'like' and 'autocomplete' in ElasticSearch. It should be synced with analyzers in remote tasks");
        public BaseValue<int> MaxResults = new BaseValue<int>("max_results", 100000);
        public BaseValue<int> MaxStatSortResults = new BaseValue<int>("max_stat_sort_results",0);
        public BaseValue<int> StatSortBulkSize = new BaseValue<int>("stat_sort_bulk_size", 5000);
        public BaseValue<int> MediaPageSize = new BaseValue<int>("media_page_size", 10000);



        public override string TcmKey => TcmObjectKeys.ElasticSearchConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}