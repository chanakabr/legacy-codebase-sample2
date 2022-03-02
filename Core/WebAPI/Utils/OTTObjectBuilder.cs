using ApiObjects.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Reflection;
using WebAPI.Models.General;

namespace WebAPI.Utils
{
    public class OTTObjectBuilder
    {
        public static dynamic buildList(Type itemType, object[] array)
        {
            Type listType = typeof(List<>).MakeGenericType(itemType);
            dynamic list = Activator.CreateInstance(listType);

            foreach (object item in array)
            {
                var parameters = item.GetType() == typeof(JObject) ? ((JObject)item).ToObject<Dictionary<string, object>>() : (Dictionary<string, object>)item;
                var obj = Deserializer.deserialize(itemType, parameters);
                list.Add((dynamic)obj);
            }

            return list;
        }

        public static dynamic buildList(Type itemType, JArray array)
        {
            Type listType = typeof(List<>).MakeGenericType(itemType);
            dynamic list = Activator.CreateInstance(listType);

            foreach (JToken item in array)
            {
                if (itemType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    var itemObject = Deserializer.deserialize(itemType, item.ToObject<Dictionary<string, object>>());

                    list.Add((dynamic)itemObject);
                }
                else
                {
                    list.Add((dynamic)Convert.ChangeType(item, itemType));
                }
            }

            return list;
        }

        public static List<T> buildList<T>(Type itemType, JArray array) where T : IKalturaOTTObject, IKalturaSerializable
        {
            List<T> list = new List<T>();

            foreach (JToken item in array)
            {
                var deserialized = Deserializer.deserialize(itemType, item.ToObject<Dictionary<string, object>>());
                T itemObject = (T)deserialized;
                list.Add(itemObject);
            }

            return list;
        }

        public static List<T> buildNativeList<T>(Type itemType, JArray array)
        {
            List<T> list = new List<T>();

            foreach (JToken item in array)
            {
                list.Add((T)Convert.ChangeType(item, itemType));
            }

            return list;
        }

        public static dynamic buildNativeList(Type itemType, object[] array)
        {
            Type listType = typeof(List<>).MakeGenericType(itemType);
            dynamic list = Activator.CreateInstance(listType);

            foreach (object item in array)
            {
                var obj = Deserializer.deserialize(itemType, item as Dictionary<string, object>);
                list.Add((dynamic)obj);
            }

            return list;
        }

        public static SerializableDictionary<string, T> buildDictionary<T>(Type itemType, Dictionary<string, object> dictionary) where T : KalturaOTTObject
        {
            SerializableDictionary<string, T> res = new SerializableDictionary<string, T>();

            foreach (string key in dictionary.Keys)
            {
                JToken item = (JToken)dictionary[key];
                T itemObject = (T)Deserializer.deserialize(itemType, item.ToObject<Dictionary<string, object>>());
                res.Add(key, itemObject);
            }

            return res;
        }

        public static dynamic buildDictionary(Type type, Dictionary<string, object> dictionary)
        {
            dynamic res = Activator.CreateInstance(type);

            Type itemType = type.GetGenericArguments()[1];
            foreach (string itemKey in dictionary.Keys)
            {
                if (itemType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    var itemValue = (JToken)dictionary[itemKey];
                    var itemObject = Deserializer.deserialize(itemType, itemValue.ToObject<Dictionary<string, object>>());
                    res.Add(itemKey, (dynamic)Convert.ChangeType(itemObject, itemType));
                }
                else
                {
                    res.Add(itemKey, (dynamic)Convert.ChangeType(dictionary[itemKey], itemType));
                }
            }

            return res;
        }

        public static DateTime longToDateTime(long unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dtDateTime.AddSeconds(unixTimeStamp);
        }
    }
}
