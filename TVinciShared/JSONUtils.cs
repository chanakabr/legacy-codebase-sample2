using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVinciShared
{
    public static class JSONUtils
    {

        public static string ToJSON(this object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static string ToJSON(this object obj, int recursionDepth)
        {
            var options = new JsonSerializerSettings
            {
                MaxDepth = recursionDepth,
            };
            
            return JsonConvert.SerializeObject(obj, options);
        }

        public static T JsonToObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static bool TryGetJSONToken(JToken baseToken, string[] orderedPathDownTheJSONTree, ref string result)
        {
            int length = orderedPathDownTheJSONTree.Length;
            JToken iter = baseToken;
            JToken temp = null;
            for (int i = 0; i < length; i++)
            {
                temp = iter.SelectToken(orderedPathDownTheJSONTree[i]);
                if (temp == null)
                    return false;
                iter = temp;
            }

            result = iter.ToString();

            return true;
        }


    }
}
