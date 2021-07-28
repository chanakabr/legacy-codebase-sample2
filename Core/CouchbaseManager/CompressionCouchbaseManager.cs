using System;
using System.Collections.Generic;
using System.Reflection;
using CouchbaseManager.Compression;
using CouchbaseManager.Extensions;
using CouchbaseManager.Models;
using KLogMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchbaseManager
{
    public class CompressionCouchbaseManager : ICompressionCouchbaseManager
    {
        private const string THIS_TYPE_OF_COMPRESSION_ISNOT_SUPPORTED_CURRENTLY = "This type of Compression isn't supported currently";
        
        private static readonly KLogger Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly ICouchbaseManager _manager;

        private readonly Compression.Compression _defaultCompression = Compression.Compression.Gzip;

        private static readonly IDictionary<Compression.Compression, Func<byte[], string>> CompressionStrategies =
            new Dictionary<Compression.Compression, Func<byte[], string>>
            {
                { Compression.Compression.Gzip, bytes => bytes.Decompress() }
            };

        private static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public CompressionCouchbaseManager(ICouchbaseManager manager)
        {
            _manager = manager;
        }
        
        public CompressionCouchbaseManager(ICouchbaseManager manager, Compression.Compression defaultCompression) : this(manager)
        {
            _defaultCompression = defaultCompression;
        }

        public bool Set<T>(CouchbaseRecord<T> record)
        {
            switch (record.Compression)
            {
                case Compression.Compression.None:
                    return Set(record.Key, record.Content, record.Expiration, record.Version);
                case Compression.Compression.Gzip:
                {
                    var internalRecord = new InternalCouchbaseRecord<byte[]>
                    {
                        Headers = new Headers
                        {
                            Compression = record.Compression
                        },
                        Content = record.Content.Compress()
                    };
                    return Set(record.Key, internalRecord, record.Expiration, record.Version);
                }
                default:
                    Log.Error(THIS_TYPE_OF_COMPRESSION_ISNOT_SUPPORTED_CURRENTLY);
                    throw new NotImplementedException(THIS_TYPE_OF_COMPRESSION_ISNOT_SUPPORTED_CURRENTLY);
            }
        }

        public bool Set<T>(string key, T value, uint expiration)
        {
            return Set(new CouchbaseRecord<T>
            {
                Key = key,
                Content = value,
                Expiration = expiration,
                Compression = _defaultCompression
            });
        }

        public bool SetWithVersion<T>(string key, T content, ulong version, uint expiration)
        {
            return Set(new CouchbaseRecord<T>
            {
                Key = key,
                Content = content,
                Version = version,
                Expiration = expiration,
                Compression = _defaultCompression
            });
        }

        private bool Set<T>(string key, T content, uint expiration, ulong? version = null)
        {
            return !version.HasValue ? _manager.Set(key, content, expiration) : _manager.SetWithVersion(key, content, version.Value, expiration);
        }

        public T Get<T>(string key, out eResultStatus status, JsonSerializerSettings settings = null)
        {
            return GetWithVersion<T>(key, out _, out status, settings);
        }

        public T Get<T>(string key, bool asJson = false)
        {
            return GetWithVersion<T>(key, out _, out _);
        }

        public T GetWithVersion<T>(string key, out ulong version, out eResultStatus status, bool asJson)
        {
            return this.GetWithVersion<T>(key, out version, out status);
        }

        public IDictionary<string, T> GetValues<T>(List<string> keys, JsonSerializerSettings jsonSerializerSettings, bool shouldAllowPartialQuery = false)
        {
            IDictionary<string, T> result = null;

            try
            {
                var values = _manager.GetValues<string>(keys, shouldAllowPartialQuery);
                if (values != null && values.Count > 0)
                {
                    result = new Dictionary<string, T>();
                    foreach (var value in values)
                    {
                        result.Add(value.Key, CompressionCouchbaseManager.DeserializeData<T>(value.Key, out _, jsonSerializerSettings, value.Value));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(CompressionCouchbaseManager)} - Failed Get on keys = {string.Join(",", keys)}", ex);
            }

            return result;
        }

        public IDictionary<string, T> GetValues<T>(List<string> keys, bool shouldAllowPartialQuery = false, bool asJson = false)
        {
            return GetValues<T>(keys, null, shouldAllowPartialQuery);
        }
        
        public bool IsKeyExists(string key)
        {
            return _manager.IsKeyExists(key);
        }

        public bool Remove(string key, ulong cas = 0)
        {
            return _manager.Remove(key, cas);
        }

        public T GetWithVersion<T>(string key, out ulong version, out eResultStatus status, JsonSerializerSettings settings = null)
        {
            status = eResultStatus.ERROR;
            var serializedResult = _manager.GetWithVersion<string>(key, out version, out status);
            if (status == eResultStatus.KEY_NOT_EXIST || status == eResultStatus.ERROR)
            {
                return default;
            }

            try
            {
                return CompressionCouchbaseManager.DeserializeData<T>(key, out status, settings, serializedResult);
            }
            catch (Exception ex)
            {
                Log.Error($"{nameof(CompressionCouchbaseManager)} - Failed Get on key = {key}", ex);
            }

            return default;
        }

        private static T DeserializeData<T>(string key, out eResultStatus status, JsonSerializerSettings settings, string serializedResult)
        {
            var parsedToken = JToken.Parse(serializedResult);
            if (parsedToken.IsObject())
            {
                if (!(parsedToken is JObject parsedObject))
                {
                    status = eResultStatus.ERROR;
                    Log.Error($"Failed to deserialize object, key = {key}");
                    return default;
                }

                var isSuccess = parsedObject.TryGetValue(InternalCouchbaseRecord<T>.HeadersPropertyName, out var headers);
                if (!isSuccess)
                {
                    status = eResultStatus.SUCCESS;
                    return DeserializeObject<T>(serializedResult, settings);
                }

                var compression = (int?) headers[Headers.CompressionPropertyName];
                if (!compression.HasValue)
                {
                    status = eResultStatus.ERROR;
                    Log.Error($"Missing parameter for Compression property, key = {key}");
                    return default;
                }

                if (compression.GetValueOrDefault() != (int) Compression.Compression.None)
                {
                    return CompressionCouchbaseManager.HandleCompressedObject<T>(key, (Compression.Compression) compression, out status, settings, parsedObject);
                }
            }

            status = eResultStatus.SUCCESS;
            return DeserializeObject<T>(serializedResult, settings);
        }

        private static T HandleCompressedObject<T>(
            string key,
            Compression.Compression compression,
            out eResultStatus status,
            JsonSerializerSettings settings,
            JObject parsedObject)
        {
            if (!CompressionStrategies.TryGetValue(compression, out var decompressFunc))
            {
                Log.Error($"There is no compression strategy for compression - {compression.ToString()}");
                status = eResultStatus.ERROR;
                return default;
            }

            if (!parsedObject.TryGetValue(nameof(InternalCouchbaseRecord<object>.Content), out var contentToken))
            {
                status = eResultStatus.ERROR;
                Log.Error($"Incorrect structure of compressed object in Couchbase, key = {key}");
                return default;
            }
            
            var decompressedValue = decompressFunc(Convert.FromBase64String(contentToken.Value<string>()));
            if (string.IsNullOrEmpty(decompressedValue))
            {
                status = eResultStatus.ERROR;
                return default;
            }

            var result = DeserializeObject<T>(decompressedValue, settings);
            status = eResultStatus.SUCCESS;
            return result;
        }

        private static T DeserializeObject<T>(string serializedResult, JsonSerializerSettings settings = null)
        {
            return JsonConvert.DeserializeObject<T>(serializedResult, settings ?? DefaultJsonSerializerSettings);
        }
    }
}