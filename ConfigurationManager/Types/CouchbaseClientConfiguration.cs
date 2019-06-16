using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class CouchbaseClientConfiguration : ConfigurationValue
    {
        private static readonly JsonSerializerSettings COUCHBASE_JSON_SERIALIZER_SETTINGS = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.None
        };

        public bool UseSsl;
        
        [JsonProperty("max_degree_of_parallelism")]
        public int MaxDegreeOfParallelism;
        
        public List<string> Servers;
        
        public Dictionary<string, CouchbaseBucketConfiguration> BucketConfigs;

        public CouchbaseClientConfiguration(string key) : base(key){ }

        internal override bool Validate()
        {
            var result = base.Validate();

            try
            {
                CouchbaseClientConfiguration couchbaseClientConfig = null;
                var configJsonStr = this.ObjectValue?.ToString();
                if (string.IsNullOrEmpty(configJsonStr))
                {
                    LogError($"Could not find config in key couchbase_client_config", ConfigurationValidationErrorLevel.Failure);
                    return false;
                }

                couchbaseClientConfig = JsonConvert.DeserializeObject<CouchbaseClientConfiguration>(this.ObjectValue.ToString(), COUCHBASE_JSON_SERIALIZER_SETTINGS);
            }
            catch (Exception ex)
            {
                LogError($"Could not parse couchbase_client_config. Error = {ex}", ConfigurationValidationErrorLevel.Failure);
                return false;
            }

            return result;
        }
    }

    public class CouchbaseBucketConfiguration
    {
        public string BucketName { get; set; }
        public bool UseSsl { get; set; }
        public string Password { get; set; }
        public long OperationLifespan { get; set; }

        public CouchbasePoolConfiguration PoolConfiguration { get; set; }

    }

    public class CouchbasePoolConfiguration
    {
        public string Name { get; set; }
        public long MaxSize { get; set; }
        public long MinSize { get; set; }
        public long SendTimeout { get; set; }
    }
}