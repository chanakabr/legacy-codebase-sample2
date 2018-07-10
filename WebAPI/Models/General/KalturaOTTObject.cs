using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Reflection;

namespace WebAPI.Models.General
{
    public interface IKalturaSerializable
    {
        string ToJson(Version currentVersion, bool omitObsolete);

        string PropertiesToXml(Version currentVersion, bool omitObsolete);
    }

    public class KalturaSerializable : IKalturaSerializable
    {
        public virtual string ToJson(Version currentVersion, bool omitObsolete)
        {
            return "{" + PropertiesToJson(currentVersion, omitObsolete) + "}";
        }

        public virtual string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            return "";
        }

        protected virtual string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            return "";
        }
    }

    public interface IKalturaOTTObject
    {
    }

    /// <summary>
    /// Base class
    /// </summary>
    public partial class KalturaOTTObject : KalturaSerializable, IKalturaOTTObject
    {
        public KalturaOTTObject(Dictionary<string, object> parameters = null)
        {
        }

        protected DateTime longToDateTime(long unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        }

        public static dynamic buildList(Type itemType, object[] array)
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

        public List<T> buildList<T>(Type itemType, JArray array) where T : KalturaOTTObject
        {
            List<T> list = new List<T>();

            foreach (JToken item in array)
            {
                T itemObject = (T)Deserializer.deserialize(itemType, item.ToObject<Dictionary<string, object>>());
                list.Add(itemObject);
            }

            return list;
        }

        public List<T> buildNativeList<T>(Type itemType, JArray array)
        {
            List<T> list = new List<T>();

            foreach (JToken item in array)
            {
                list.Add((T) Convert.ChangeType(item, itemType));
            }

            return list;
        }

        public SerializableDictionary<string, T> buildDictionary<T>(Type itemType, Dictionary<string, object> dictionary) where T : KalturaOTTObject
        {
            SerializableDictionary<string, T> res = new SerializableDictionary<string, T>();

            foreach (string key in dictionary.Keys)
            {
                JToken item = (JToken)dictionary[key];
                T itemObject = (T) Deserializer.deserialize(itemType, item.ToObject<Dictionary<string, object>>());
                res.Add(key, itemObject);
            }

            return res;
        }

        public static dynamic buildDictionary(Type type, Dictionary<string, object> dictionary)
        {
            dynamic res = Activator.CreateInstance(type);

            Type itemType = type.GetGenericArguments()[1];
            foreach (string key in dictionary.Keys)
            {
                JToken item = (JToken)dictionary[key];

                if (itemType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    var itemObject = Deserializer.deserialize(itemType, item.ToObject<Dictionary<string, object>>());
                    res.Add(key, (dynamic)Convert.ChangeType(itemObject, itemType));
                }
                else
                {
                    res.Add(key, (dynamic)Convert.ChangeType(dictionary[key], itemType));
                }
            }

            return res;
        }

        [DataMember(Name = "objectType")]
        [JsonProperty(PropertyName = "objectType")]
        [XmlElement(ElementName = "objectType")]
        public string objectType { get { return this.GetType().Name; } set { } }

        [DataMember(Name = "relatedObjects")]
        [JsonProperty(PropertyName = "relatedObjects")]
        [XmlElement(ElementName = "relatedObjects")]
        public SerializableDictionary<string, KalturaListResponse> relatedObjects
        {
            get;
            set;
        }

        public virtual void AfterRequestParsed(string service, string action, string language, int groupId, string userId, string deviceId, JObject json = null)
        {

        }
    }
}