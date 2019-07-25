using ApiObjects.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        string ToJson(Version currentVersion, bool omitObsolete);

        string ToXml(Version currentVersion, bool omitObsolete);
    }

    public class KalturaSerializable : IKalturaSerializable
    {
        private static Dictionary<string, string> escapeMapping = new Dictionary<string, string>()
        {
            {"\"", @"\"""},
            {"\\\\", @"\\"},
            {"\a", @"\a"},
            {"\b", @"\b"},
            {"\f", @"\f"},
            {"\n", @"\n"},
            {"\r", @"\r"},
            {"\t", @"\t"},
            {"\v", @"\v"},
            {"\0", @"\0"},
        };
        private static Regex escapeRegex = new Regex(string.Join("|", escapeMapping.Keys.ToArray()));

        protected string EscapeJson(string str)
        {
            return escapeRegex.Replace(str, EscapeMatchEval);
        }

        private static string EscapeMatchEval(Match m)
        {
            if (escapeMapping.ContainsKey(m.Value))
            {
                return escapeMapping[m.Value];
            }
            return escapeMapping[Regex.Escape(m.Value)];
        }

        protected string EscapeXml(string str)
        {
            return WebUtility.HtmlEncode(str);
        }

        public virtual string ToJson(Version currentVersion, bool omitObsolete)
        {
            return "{" + String.Join(", ", PropertiesToJson(currentVersion, omitObsolete).Values) + "}";
        }

        public virtual string ToXml(Version currentVersion, bool omitObsolete)
        {
            return String.Join("", PropertiesToXml(currentVersion, omitObsolete).Values);
        }

        protected virtual Dictionary<string, string> PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            return new Dictionary<string, string>();
        }

        protected virtual Dictionary<string, string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            return new Dictionary<string, string>();
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
        public U GetItemsIn<U, T>(string itemsIn, string propertyName, bool checkDuplicate = false, bool ignoreDefaultValueValidation = false) where T : IConvertible where U : ICollection<T>
        {
            U values = Activator.CreateInstance<U>();

            if (!string.IsNullOrEmpty(itemsIn))
            {
                string[] stringValues = itemsIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                Type t = typeof(T);
                foreach (string stringValue in stringValues)
                {
                    T value;

                    try
                    {
                        if (t.IsEnum)
                        {
                            value = (T)Enum.Parse(t, stringValue);
                        }
                        else
                        {
                            value = (T)Convert.ChangeType(stringValue, t);
                        }
                    }
                    catch (Exception)
                    {
                        throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, propertyName, t.Name);
                    }

                    if (value != null && (ignoreDefaultValueValidation || !value.Equals(default(T))))
                    {
                        if (!values.Contains(value))
                        {
                            values.Add(value);
                        }
                        else if (checkDuplicate)
                        {
                            throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, propertyName);
                        }
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, propertyName);
                    }
                }
            }

            return values;
        }
    }
    
    /// <summary>
    /// Base class
    /// </summary>
    public class KalturaOTTFile
    {
        public KalturaOTTFile(string filePath, string fileName)
        {
            path = filePath;
            name = fileName;
        }

        public string path { get; set; }
        public string name { get; set; }
    }

    public partial class KalturaOTTObjectSupportNullable : KalturaOTTObject
    {
        internal HashSet<string> NullableProperties { get; private set; }

        public void AddNullableProperty(string str)
        {
            if (NullableProperties == null)
                NullableProperties = new HashSet<string>();

            NullableProperties.Add(str.ToLower());
        }
    }
}