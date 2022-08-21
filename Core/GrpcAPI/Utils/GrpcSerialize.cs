using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using phoenix;
using Phx.Lib.Log;
using ProtoBuf;
using ProtoBuf.Meta;

namespace GrpcAPI.Utils
{
    public class GrpcSerialize
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.ToString());

        public static byte[] ProtoSerialize<T>(T record) where T : class  
        {  
            if (null == record) return null;  
  
            try  
            {  
                using (var stream = new MemoryStream())
                {
                    RuntimeTypeModel.Default.Serialize(stream, record);  
                    return stream.ToArray();  
                }  
            }  
            catch  
            { 
                Logger.LogError($"ProtoSerialize for {typeof(T)} failed");
                throw;  
            }  
        }
        public static T ProtoDeserialize<T>(byte[] data) where T : class  
        {  
            if (null == data) return null;  
  
            try  
            {  
                using (var stream = new MemoryStream(data))  
                {
                    return RuntimeTypeModel.Default.Deserialize<T>(stream);  
                }  
            }  
            catch  
            { 
                Logger.LogError($"ProtoDeserialize for {typeof(T)} failed");
                throw;  
            }  
        } 
    }
}