using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ConfigurationManager.Types
{
    public class CouchbaseClientConfiguration : BaseConfig<CouchbaseClientConfiguration>
    {
        public override string TcmKey => TcmObjectKeys.CouchbaseClientConfiguration;
        public override string[] TcmPath => new string[] { TcmKey };

        private static readonly List<string> defaultBucketServerList = new List<string>()
        {
            "http://couchbase6_1:8091/pools",
            "http://couchbase6_2:8091/pools",
            "http://couchbase6_3:8091/pools"
        };
        public BaseValue<bool> UseSsl = new BaseValue<bool>("UseSsl", false);
        public BaseValue<int> MaxDegreeOfParallelism = new BaseValue<int>("max_degree_of_parallelism", 4);
        public BaseValue<List<string>> Servers = new BaseValue<List<string>>("Servers", defaultBucketServerList);
        public BucketsConfig BucketsConfig= new BucketsConfig();

    }


    public class BucketsConfig : BaseConfig<BucketsConfig>
    {
        public override string TcmKey => TcmObjectKeys.CouchbaseBucketConfiguration;

        public override string[] TcmPath => new string[] { TcmObjectKeys.CouchbaseClientConfiguration, TcmKey };

        private static readonly CouchbaseBucketConfig defaultCouchbaseBucketConfiguration = new CouchbaseBucketConfig();
        public Dictionary<string, CouchbaseBucketConfig> BucketConfigs = new Dictionary<string, CouchbaseBucketConfig>()
        {
            {TcmObjectKeys.DefaultConfigurationKey, defaultCouchbaseBucketConfiguration }
        };

        public void SetValues(JToken token, Dictionary<string, CouchbaseBucketConfig> bucketsConfiguration)
        {
            if (token == null)
            {
                _Logger.Info($"Empty data in TCM under object:  [{GetType().Name}]  for key [{string.Join(":", TcmPath) }], setting default value as actual value");
                return;
            }
            CouchbaseBucketConfig defaultConfig = bucketsConfiguration[TcmObjectKeys.DefaultConfigurationKey];
            JObject tokenConfiguration = JObject.Parse(token.ToString());
            var defaultTokenData = tokenConfiguration[TcmObjectKeys.DefaultConfigurationKey];
            InitDefaultBucketConfig(defaultTokenData, defaultConfig);

            foreach (KeyValuePair<string, JToken> pair in tokenConfiguration)
            {
                if(pair.Key == TcmObjectKeys.DefaultConfigurationKey)
                {
                    continue;//already init at the top 
                }
                CouchbaseBucketConfig newConfig = defaultConfig.DeepCopy();
                if (!bucketsConfiguration.TryGetValue(pair.Key, out var currentConfig))
                {
                    bucketsConfiguration.Add(pair.Key, newConfig);
                }

                List<FieldInfo> fields = newConfig.GetType().GetFields().ToList();
                newConfig.PoolConfiguration.AddPath(pair.Key);
                IterateOverClassFields(newConfig, tokenConfiguration[pair.Key]);
                bucketsConfiguration[pair.Key] = newConfig;
            }
        }

        private void InitDefaultBucketConfig(JToken defaultTokenData, CouchbaseBucketConfig defaultBucketConfig)
        {
            defaultBucketConfig.PoolConfiguration.AddPath(TcmObjectKeys.DefaultConfigurationKey);
            IterateOverClassFields(defaultBucketConfig, defaultTokenData);
        }
    }

    public class CouchbaseBucketConfig : BaseConfig<CouchbaseBucketConfig>, IDeepCopyConverter<CouchbaseBucketConfig>
    {
        public BaseValue<string> BucketName = new BaseValue<string>("bucketName", "Default");
        public BaseValue<bool> UseSsl = new BaseValue<bool>("useSsl", false);
        public BaseValue<string> Password = new BaseValue<string>("password", null);
        public BaseValue<long> OperationLifespan = new BaseValue<long>("operationLifespan", 20000);
        public CouchbasePoolConfiguration PoolConfiguration = new CouchbasePoolConfiguration();
        

        public override string TcmKey => TcmObjectKeys.DefaultConfigurationKey;

        public override string[] TcmPath => new string[] { TcmObjectKeys.CouchbaseClientConfiguration,  TcmKey };


        public CouchbaseBucketConfig DeepCopy()
        {
            CouchbaseBucketConfig res = new CouchbaseBucketConfig()
            {
                BucketName = BucketName.DeepCopy(),
                OperationLifespan = OperationLifespan.DeepCopy(),
                Password = Password.DeepCopy(),
                UseSsl = UseSsl.DeepCopy(),
                PoolConfiguration = PoolConfiguration.DeepCopy(),
            };
            return res;
        }
    }

    public class CouchbasePoolConfiguration : BaseConfig<CouchbaseBucketConfig>, IDeepCopyConverter<CouchbasePoolConfiguration>
    {
        public BaseValue<string> Name = new BaseValue<string>("name", "custom");
        public BaseValue<long> MaxSize = new BaseValue<long>("maxSize", 25);
        public BaseValue<long> MinSize = new BaseValue<long>("minSize", 5);
        public BaseValue<long> SendTimeout = new BaseValue<long>("sendTimeout", 12000);

        public override string TcmKey => TcmObjectKeys.CouchbasePoolConfiguration;

        public override string[] TcmPath => path;

        private string[] path;
        public void AddPath(string bucketName)
        {
            path = new string[] { TcmObjectKeys.CouchbaseClientConfiguration, TcmObjectKeys.CouchbaseBucketConfiguration, bucketName, TcmKey };

        }
        public CouchbasePoolConfiguration DeepCopy()
        {
            var res = new CouchbasePoolConfiguration()
            {
                MaxSize = MaxSize.DeepCopy(),
                MinSize = MinSize.DeepCopy(),
                Name = Name.DeepCopy(),
                SendTimeout = SendTimeout.DeepCopy()
            };
            return res;
        }
    }
}