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

namespace WebAPI.Models.General
{
    public interface IKalturaSerializable
    {
        string ToJson(Version currentVersion, bool omitObsolete, bool responseProfile = false);

        string ToXml(Version currentVersion, bool omitObsolete, bool responseProfile = false);
    }

    public interface IKalturaOTTObject
    {
        void AfterRequestParsed(string service, string action, string language, int groupId, string userId, string deviceId, JObject json = null);
    }
    
    /// <summary>
    /// Base class
    /// </summary>
    public partial class KalturaOTTObject : KalturaSerializable, IKalturaOTTObject
    {
        [DataMember(Name = "objectType")]
        [JsonProperty(PropertyName = "objectType")]
        [XmlElement(ElementName = "objectType")]
        public string objectType { get { return this.GetType().Name; } set { } }

        [DataMember(Name = "relatedObjects")]
        [JsonProperty(PropertyName = "relatedObjects")]
        [XmlElement(ElementName = "relatedObjects")]
        public SerializableDictionary<string, IKalturaListResponse> relatedObjects { get; set; }

        public virtual void SetRelatedObjects(ContextData contextData, KalturaDetachedResponseProfile profile) { }
        
        public KalturaOTTObject(Dictionary<string, object> parameters = null)
        {
            Init();
        }

        protected virtual void Init()
        {
        }

        protected DateTime longToDateTime(long unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            return dtDateTime.AddSeconds(unixTimeStamp);
        }

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

        public List<T> buildList<T>(Type itemType, JArray array) where T : IKalturaOTTObject, IKalturaSerializable
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

        public List<T> buildNativeList<T>(Type itemType, JArray array)
        {
            List<T> list = new List<T>();

            foreach (JToken item in array)
            {
                list.Add((T) Convert.ChangeType(item, itemType));
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
        
        public virtual void AfterRequestParsed(string service, string action, string language, int groupId, string userId, string deviceId, JObject json = null)
        {

        }

        /// <summary>
        /// Convert comma separated string to collection.
        /// </summary>
        /// <typeparam name="U">Collection of T</typeparam>
        /// <typeparam name="T">Type of items in collection</typeparam>
        /// <param name="itemsIn">Comma separated string</param>
        /// <param name="propertyName">The property name of comma separated string (for error message)</param>
        /// <returns></returns>
        public U GetItemsIn<U, T>(string itemsIn, string propertyName, bool checkDuplicate = false,
            bool ignoreDefaultValueValidation = false)
            where T : IConvertible
            where U : ICollection<T> => Utils.Utils.ParseCommaSeparatedValues<U, T>(
            itemsIn,
            propertyName,
            checkDuplicate,
            ignoreDefaultValueValidation);
    }
}