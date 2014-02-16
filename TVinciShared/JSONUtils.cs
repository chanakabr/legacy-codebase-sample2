using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace TVinciShared
{
    public static class JSONUtils
    {

        public static string ToJSON(this object obj)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Serialize(obj);
        }

        public static string ToJSON(this object obj, int recursionDepth)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.RecursionLimit = recursionDepth;
            return serializer.Serialize(obj);
        }

        public static T JsonToObject<T>(string json)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            T obj = js.Deserialize<T>(json);
            return (T)obj;  
        }

        public static bool TryGetJSONToken(JToken baseToken, string[] orderedPathDownTheJSONTree, ref string result)
        {
            int length = orderedPathDownTheJSONTree.Length;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(String.Concat(i == 0 ? string.Empty : ".", orderedPathDownTheJSONTree[i]));
                if (baseToken.SelectToken(sb.ToString()) == null)
                    return false;
            }

            result = baseToken.SelectToken(sb.ToString()).ToString();

            return true;
        }


    }
}
