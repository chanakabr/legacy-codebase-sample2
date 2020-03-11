using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;

namespace ConfigurationManager
{
    public abstract class BaseCacheConfigurationParams : BaseConfig<BaseCacheConfigurationParams>
    {
        public BaseValue<string> Type = new BaseValue<string>("type", "CouchBase");
        public BaseValue<string> Name = new BaseValue<string>("name", "Cache");
        public BaseValue<int> TTLSeconds = new BaseValue<int>("ttl_seconds", 86400);



    }

    public class BaseCacheConfiguration : BaseCacheConfigurationParams
    {
        public override string TcmKey => TcmObjectKeys.BaseCacheConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        
    }


    public class WSCacheConfiguration : BaseCacheConfigurationParams
    {
        public override string TcmKey => TcmObjectKeys.WSCacheConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
        

        public new  BaseValue<int> TTLSeconds  = new BaseValue<int>("ttl_seconds", 7200);
        
        
    }

    public class ODBCWrapperCacheConfiguration : BaseCacheConfigurationParams
    {
        public override string TcmKey => TcmObjectKeys.ODBCWrapperCacheConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public new BaseValue<int> TTLSeconds = new BaseValue<int>("ttl_seconds", 7200);

    }


    public class CatalogCacheConfiguration : BaseCacheConfigurationParams
    {
        public override string TcmKey => TcmObjectKeys.CatalogCacheConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };

        public new BaseValue<int> TTLSeconds = new BaseValue<int>("ttl_seconds", 3600);
        public new BaseValue<string> Name = new BaseValue<string>("name", "CatalogCache");

    }


    public class NotificationCacheConfiguration : BaseCacheConfigurationParams
    {
        public override string TcmKey => TcmObjectKeys.NotificationCacheConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };


        public new BaseValue<int> TTLSeconds = new BaseValue<int>("ttl_seconds", 3600);
        public new BaseValue<string> Name = new BaseValue<string>("name", "NotificationCache");
    }

    public class GroupsCacheConfiguration : BaseCacheConfigurationParams
    {
        public override string TcmKey => TcmObjectKeys.GroupsCacheConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };


        public new BaseValue<int> TTLSeconds = new BaseValue<int>("ttl_seconds", 86400);
        public new BaseValue<string> Name = new BaseValue<string>("name", "GroupsCache");

    }

}