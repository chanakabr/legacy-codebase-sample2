using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using EventBus.Abstraction;
using Newtonsoft.Json;

namespace EventBus.RabbitMQ
{
    public static class RabbitMqSerializationsHelper
    {
        public static byte[] Serialize(object obj)
        {
            using (var stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                formatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }
        
        public static ServiceEvent Deserialize(byte[] message, Type eventType)
        {
            object result;
            try
            {
                result = BinaryDeserialize(message);
            }
            catch (SerializationException e)
            {
                //Gil:this is a temp fix until ingest handlers and phoenix are going to be synced so we need to have a
                //backwards compatibility to suppport json serialized data (we changed to binarry formatter)
                //if you get this  it wil use json deserializer instead
                result = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message), eventType);
            }
            catch(Exception e)
            {
                throw;
            }
            
            return (ServiceEvent)result;
        }
        
        private static object BinaryDeserialize(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(stream);
            }
        }
        
    }
}