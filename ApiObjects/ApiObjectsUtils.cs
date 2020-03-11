using System;
using System.Text;

namespace ApiObjects
{
    public class ApiObjectsUtils
    {
        public static string Base64Encode(string stringToEncode)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(stringToEncode);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
