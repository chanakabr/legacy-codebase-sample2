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
