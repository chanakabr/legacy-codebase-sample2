using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SoapAdaptersCommon.Helpers
{
    public static class RequestHasher
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Generates a unique reproducable signature give an invoked method and a list of arguments
        /// </summary>
        /// <param name="methodInfo">The Service contract MethodInfo that is beeing invoked</param>
        /// <param name="inputs">The list of Arguments sent to the method above</param>
        /// <returns>A unique signature of the invoked request</returns>
        public static string GetRequestSignature(MethodBase methodInfo, object[] inputs)
        {
            var methodInvocationInfo = new Dictionary<string, object>
            {
                ["methodSignature"] = methodInfo.Name,
                ["args"] = inputs
            };

            var methodInvocationInfoBuffer = BinarySerialize(methodInvocationInfo);
            var signature = CreateMD5(methodInvocationInfoBuffer);

            _Logger.Debug($"signature is:[{signature}]");
            return signature;
        }


        private static byte[] BinarySerialize(object obj)
        {
            var stringMessage = JsonConvert.SerializeObject(obj, Formatting.None);
            var bytes = Encoding.UTF8.GetBytes(stringMessage);
            return bytes;
        }

        private static string CreateMD5(byte[] inputBytes)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
    }
}
