using KLogMonitor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace RestAdaptersCommon
{
    public static class CryptoHelpers
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string SEPERATOR_FOR_ENUMRABLE_VALUES="";

        public static string HmacSha1(this string value, string secret)
        {
            var secretByteArray = Encoding.UTF8.GetBytes(secret);
            using (var hash = new HMACSHA1(secretByteArray))
            {
                var signatureBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
                var signature = Convert.ToBase64String(signatureBytes);
                return signature;
            }
        }

        public static string CalculateSignature(this BaseAdapterRequest req, string secret)
        {
            var type = req.GetType();
            var props = type.GetProperties().Where(p => !Attribute.IsDefined(p, typeof(IgnoreFromSignatureHash)));
            string values = GetConcatenatedValues(req, props);
            var signature = values.HmacSha1(secret);
            return signature;
        }

        private static string GetConcatenatedValues(object obj, IEnumerable<PropertyInfo> props)
        {
            var sb = new StringBuilder();
            foreach (var prop in props)
            {
                var strValue = "";
                var propValue = prop.GetValue(obj);
                if (propValue != null)
                {
                    if (propValue is IEnumerable enumrableValue)
                    {
                        strValue = string.Join(SEPERATOR_FOR_ENUMRABLE_VALUES, enumrableValue);
                    }
                    else
                    {
                        strValue = propValue.ToString();
                    }
                }

                sb.Append(strValue);
            }
            var values = sb.ToString();
            return values;
        }
    }
}
