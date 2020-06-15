using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;
using System;

namespace ConfigurationManager
{
    public class ElasticSearchHandlerConfiguration : BaseConfig<ElasticSearchHandlerConfiguration>
    {
        public BaseValue<int> BulkSize = new BaseValue<int>("bulk_size", 1000, false, "Number of documents to be updated in same ElasticSearch bulk when rebuilding the index. " +
                "This value can be several hundreds, depending on typical document size and machine capabilities");
        public BaseValue<int> NumberOfShards = new BaseValue<int>("shards", 4);
        public BaseValue<int> NumberOfReplicas = new BaseValue<int>("replicas", 1);
        public BaseValue<int> ChannelStartDateDays = new BaseValue<int>("channel_start_date_days", 30, false, "Used in EPG Channel updater (when getting programs by channel Ids and dates)");
        public BaseValue<int> GetGroupMediaTimeout = new BaseValue<int>("get_group_media_timeout", 90, false, "When running Get_GroupMedias_ml stored procedure, how much time (in seconds) should code wait until receiving timeout exception");
        public BaseValue<int> MediaPageSize = new BaseValue<int>("media_page_size", 1000, false, "Number of medias to fetch from DB on each GetGroupMediaAssets stored procedure execution");
        public BaseValue<int> EpgPageSize = new BaseValue<int>("epg_page_size", 1000, false, "Number of epgs to fetch from CB on each query to group_programs view");

        public override string TcmKey => TcmObjectKeys.ElasticsearchHandlerConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };


    }
}
