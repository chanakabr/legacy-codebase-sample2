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
        private static  Dictionary<string, CouchbaseBucketConfig> bucketDefaultConfigs = new Dictionary<string, CouchbaseBucketConfig>()
        {
            {TcmObjectKeys.DefaultConfigurationKey, defaultCouchbaseBucketConfiguration }
        };

        public BaseValue<Dictionary<string, CouchbaseBucketConfig>> BucketConfigs = new BaseValue<Dictionary<string, CouchbaseBucketConfig>>(TcmObjectKeys.CouchbaseBucketConfiguration, bucketDefaultConfigs);



        public override void SetActualValue<TV>(JToken token, BaseValue<TV> defaultData)
        {
            Dictionary<string, CouchbaseBucketConfig> actual = new Dictionary<string, CouchbaseBucketConfig>();
            var defaultConfiguration = defaultData.DefaultValue as Dictionary<string, CouchbaseBucketConfig>;

            if (token == null)
            {
                _Logger.Info($"Empty data in TCM under object: [{GetType().Name}]  for key [{string.Join(":", TcmPath) }], setting default value as actual value");
                return;
            }

            CouchbaseBucketConfig defaultConfig = defaultConfiguration[TcmObjectKeys.DefaultConfigurationKey];
            JObject tokenConfiguration = JObject.Parse(token.ToString());
            var defaultTokenData = tokenConfiguration[TcmObjectKeys.DefaultConfigurationKey];
            InitDefaultBucketConfig(defaultTokenData, defaultConfig);

            actual.Add(TcmObjectKeys.DefaultConfigurationKey, defaultConfig);

            foreach (KeyValuePair<string, JToken> pair in tokenConfiguration)
            {
                if (pair.Key == TcmObjectKeys.DefaultConfigurationKey)
                {
                    continue;//already init at the top 
                }
                
                if (defaultConfiguration.TryGetValue(pair.Key, out var currentConfig))
                {
                    actual.Add(pair.Key, currentConfig);
                }
                else
                {
                    CouchbaseBucketConfig newConfig = CouchbaseBucketConfig.Copy(defaultConfig);
                    actual.Add(pair.Key, newConfig);
                }
                var newCalue = actual[pair.Key];
                List<FieldInfo> fields = newCalue.GetType().GetFields().ToList();
                newCalue.PoolConfiguration.AddPath(pair.Key);
                IterateOverClassFields(newCalue, tokenConfiguration[pair.Key]);
            }

            SetActualValue(defaultData as BaseValue<Dictionary<string, CouchbaseBucketConfig>>, actual);
        }

         
        private void InitDefaultBucketConfig(JToken defaultTokenData, CouchbaseBucketConfig defaultBucketConfig)
        {
            defaultBucketConfig.PoolConfiguration.AddPath(TcmObjectKeys.DefaultConfigurationKey);
            IterateOverClassFields(defaultBucketConfig, defaultTokenData);
        }
    }

    public class CouchbaseBucketConfig : BaseConfig<CouchbaseBucketConfig>
    {
        public BaseValue<string> BucketName = new BaseValue<string>("bucketName", "Default");
        public BaseValue<bool> UseSsl = new BaseValue<bool>("useSsl", false);
        public BaseValue<string> Password = new BaseValue<string>("password", null);
        public BaseValue<long> OperationLifespan = new BaseValue<long>("operationLifespan", 20000);
        public CouchbasePoolConfiguration PoolConfiguration = new CouchbasePoolConfiguration();
        

        public override string TcmKey => TcmObjectKeys.DefaultConfigurationKey;

        public override string[] TcmPath => new string[] { TcmObjectKeys.CouchbaseClientConfiguration,  TcmKey };


        internal  static CouchbaseBucketConfig Copy(CouchbaseBucketConfig copyFrom)
        {
            CouchbaseBucketConfig res = new CouchbaseBucketConfig()
            {
                BucketName = copyFrom.BucketName,
                OperationLifespan = copyFrom.OperationLifespan,
                Password = copyFrom.Password,
                UseSsl = copyFrom.UseSsl,
                PoolConfiguration = CouchbasePoolConfiguration.Copy(copyFrom.PoolConfiguration)
            };
            return res;
            
        }

    }

    public class CouchbasePoolConfiguration : BaseConfig<CouchbaseBucketConfig>
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


        internal static CouchbasePoolConfiguration Copy(CouchbasePoolConfiguration copyFrom)
        {
            CouchbasePoolConfiguration res = new CouchbasePoolConfiguration()
            {
                MaxSize = copyFrom.MaxSize,
                MinSize = copyFrom.MinSize,
                Name = copyFrom.Name,
                path = copyFrom.path,
                SendTimeout = copyFrom.SendTimeout
            };
            return res;
        }
    }
}