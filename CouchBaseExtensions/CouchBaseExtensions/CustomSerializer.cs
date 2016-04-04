using Couchbase.Core.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;

namespace CouchBaseExtensions
{
    public class CustomSerializer : ITypeSerializer
    {

        Couchbase.Core.Serialization.DefaultSerializer DefaultSerializator;
        const string groupAssemblyName = "GroupsCacheManager.Group";

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
            try
            {
                if (typeof(T).ToString() == groupAssemblyName)
                {
                    T value = default(T);
                    using (MemoryStream stream = new MemoryStream(buffer))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        stream.Seek(offset, SeekOrigin.Begin);            
                        value = (T)formatter.Deserialize(stream);
                    }
                    return value;
                }
            }

            catch (Exception ex)
            {
                if (ex != null && ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                throw new Exception("Error in Deserialize for CustomSerializer, Exception: {0}", ex);
            }

            return DefaultSerializator.Deserialize<T>(buffer, offset, length);
        }

        public byte[] Serialize(object obj)
        {
            try
            {
                if (obj.GetType().ToString() == groupAssemblyName)
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    byte[] objBytes = null;
                    using (MemoryStream stream = new MemoryStream())
                    {
                        formatter.Serialize(stream, obj);
                        objBytes = stream.ToArray();
                    }
                    return objBytes;
                }
            }

            catch (Exception ex)
            {
                if (ex != null && ex.InnerException != null)
                {
                    ex = ex.InnerException;
                }

                throw new Exception("Error in Serialize for CustomSerializer, Exception: {0}", ex);
            }

            return DefaultSerializator.Serialize(obj);
        }
    }
}