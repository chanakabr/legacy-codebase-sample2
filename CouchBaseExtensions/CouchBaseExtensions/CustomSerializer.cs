using Couchbase.Core.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CouchBaseExtensions
{
    public class CustomSerializer : ITypeSerializer
    {

        Couchbase.Core.Serialization.DefaultSerializer DefaultSerializator;

        public CustomSerializer()
        {
        }

        public CustomSerializer(JsonSerializerSettings deserializationSettings, JsonSerializerSettings serializerSettings)
        {
            DefaultSerializator = new DefaultSerializer(deserializationSettings, serializerSettings);
        }

        public T Deserialize<T>(System.IO.Stream stream)
        {
            return DefaultSerializator.Deserialize<T>(stream);
        }

        public T Deserialize<T>(byte[] buffer, int offset, int length)
        {
            return DefaultSerializator.Deserialize<T>(buffer, offset, length);
        }

        public byte[] Serialize(object obj)
        {
            return DefaultSerializator.Serialize(obj);
        }
    }
}