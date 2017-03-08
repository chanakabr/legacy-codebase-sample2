using Jil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebAPI.Exceptions;
using WebAPI.Models;
using WebAPI.Models.General;
using WebAPI.Managers.Models;
using KLogMonitor;
using WebAPI.Managers.Scheme;
using System.Dynamic;
using Newtonsoft.Json;
using WebAPI.App_Start;
using WebAPI.Reflection;
using Newtonsoft.Json.Linq;

namespace WebAPI.Utils
{
    class OldStandardObject : DynamicObject
    {
        private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

        [JsonIgnore]
        public Dictionary<string, object> Extra { get { return _fields; } }

        public OldStandardObject(KalturaOTTObject ottObject)
        {
            Type type = ottObject.GetType();
            Dictionary<string, string> oldStandardProperties = OldStandardAttribute.getOldMembers(type);

            var properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(ottObject);
                if (value == null || DataModel.isNewStandardOnly(property))
                    continue;

                string name = getApiName(property);

                if (property.PropertyType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    value = new OldStandardObject((KalturaOTTObject)value);
                }
                else if (property.PropertyType.IsArray || property.PropertyType.IsGenericType)
                {
                    if (property.PropertyType.GetGenericArguments().Count() == 1 && (property.PropertyType.GetGenericArguments()[0].IsSubclassOf(typeof(KalturaOTTObject))))
                    {
                        var array = new List<OldStandardObject>();
                        foreach(KalturaOTTObject item in (IEnumerable<KalturaOTTObject>)value)
                        {
                            array.Add(new OldStandardObject(item));
                        }
                        value = array;
                    }
                    else if (property.PropertyType.GetGenericArguments().Count() == 2 && property.PropertyType.GetGenericArguments()[1].IsSubclassOf(typeof(KalturaOTTObject)))
                    {
                        Type itemType = property.PropertyType.GetGenericArguments()[1];
                        var dictionary = new Dictionary<string, OldStandardObject>();
                        foreach (dynamic item in (dynamic)value)
                        {
                            string itemKey = item.Key;
                            KalturaOTTObject itemValue = item.Value;
                            dictionary.Add(itemKey, new OldStandardObject(itemValue));
                        }
                        value = dictionary;
                    }
                }

                _fields[name] = value;
                if (oldStandardProperties != null && oldStandardProperties.ContainsKey(name))
                {
                    _fields[oldStandardProperties[name]] = value;
                }
            }
        }

        private KeyValuePair<string, T> castPair<T>(T type, object value)
        {
            return (KeyValuePair<string, T>)value;
        }

        private string getApiName(PropertyInfo property)
        {
            return DataModel.getApiName(property);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return Extra.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return _fields.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _fields[binder.Name] = value;
            return true;
        }
    }

    public class JilFormatter : MediaTypeFormatter
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly Options _jilOptions;

        public JilFormatter()
        {
            _jilOptions = new Options(dateFormat: DateTimeFormat.SecondsSinceUnixEpoch, excludeNulls: true, includeInherited: true);
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            MediaTypeMappings.Add(new QueryStringMapping("format", "1", "application/json"));

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
            
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                Converters = new List<JsonConverter> { new MultiStringJsonConverter(), new EnumConverter(), new DoubleConverter() },
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task.FromResult(this.DeserializeFromStream(type, readStream));
        }

        private object DeserializeFromStream(Type type, Stream readStream)
        {
            try
            {
                using (var reader = new StreamReader(readStream))
                {
                    MethodInfo method = typeof(JSON).GetMethod("Deserialize", new Type[] { typeof(TextReader), typeof(Options) });
                    MethodInfo generic = method.MakeGenericMethod(type);
                    return generic.Invoke(this, new object[] { reader, _jilOptions });
                }
            }
            catch
            {
                throw new BadRequestException();
            }
        }

        public class MultiStringJsonConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                JToken jToken = value.ToString();
                jToken.WriteTo(writer);

                string language = Utils.GetLanguageFromRequest();
                if (language == null || !language.Equals("*"))
                {
                    return;
                }

                string name = writer.Path.Substring(writer.Path.LastIndexOf('.') + 1);
                string multilingualName = string.Format("multilingual{0}{1}", name.Substring(0, 1).ToUpper(), name.Substring(1));

                KalturaMultilingualString multilingualString = (KalturaMultilingualString)value;
                if (multilingualString.Values != null)
                {
                    writer.WritePropertyName(multilingualName);
                    jToken = JToken.FromObject(multilingualString.Values);
                    jToken.WriteTo(writer);
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(KalturaMultilingualString);
            }
        }

        public class EnumConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                Type type = value.GetType();
                JToken jToken;

                if (type.IsEnum)
                {
                    jToken = Enum.GetName(type, value);
                }
                else
                {
                    jToken = Enum.GetName(Nullable.GetUnderlyingType(type), value);
                }
                jToken.WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType.IsEnum || (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>) && Nullable.GetUnderlyingType(objectType).IsEnum);
            }
        }

        public class DoubleConverter : JsonConverter
        {
            private JToken GetToken(double value)
            {
                if ((value % 1) == 0)
                {
                    return (int)value;
                }

                return value;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                JToken jToken = null;
                if (value.GetType().IsGenericType)
                {
                    double? nullableValue = (double?)value;
                    if (!nullableValue.HasValue)
                    {
                        jToken = GetToken(nullableValue.Value);
                    }
                }
                else
                {
                    jToken = GetToken((double)value);
                }

                jToken.WriteTo(writer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(double) || (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>) && Nullable.GetUnderlyingType(objectType) == typeof(double));
            }
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                if (type == typeof(StatusWrapper) && ((StatusWrapper)value).Result != null)
                {
                    object result = ((StatusWrapper)value).Result;
                    StatusWrapper statusWrapper = ((StatusWrapper)value);
                    if (result.GetType().IsSubclassOf(typeof(KalturaOTTObject)))
                    {
                        if (OldStandardAttribute.isCurrentRequestOldVersion())
                        {
                            dynamic oldStandardObject = new OldStandardObject((KalturaOTTObject)result);
                            statusWrapper.Result = oldStandardObject;
                        }
                    }
                    else if (result.GetType().IsGenericType && result.GetType().GetGenericArguments()[0].IsSubclassOf(typeof(KalturaOTTObject)))
                    {
                        List<OldStandardObject> list = new List<OldStandardObject>();
                        foreach (KalturaOTTObject item in (dynamic)result)
                        {
                            list.Add(new OldStandardObject(item));
                        }
                        statusWrapper.Result = list;
                    }
                    else if (result.GetType().IsArray) // is multirequest
                    {
                        List<object> list = new List<object>();
                        foreach (object item in (dynamic)result)
                        {
                            if (item.GetType().IsSubclassOf(typeof(KalturaOTTObject)))
                            {
                                list.Add(item);
                            }
                            else if (item.GetType().IsSubclassOf(typeof(ApiException)))
                            {
                                list.Add(WrappingHandler.prepareExceptionResponse(((ApiException)item).Code, ((ApiException)item).Message));
                            }
                            else if (item.GetType().IsSubclassOf(typeof(Exception)))
                            {
                                InternalServerErrorException ex = new InternalServerErrorException();
                                list.Add(WrappingHandler.prepareExceptionResponse(ex.Code, ex.Message));
                            }
                            else
                            {
                                list.Add(item);
                            }
                        }
                        statusWrapper.Result = list;
                    }
                }

                string json = JsonConvert.SerializeObject(value);
                streamWriter.Write(json);
                
                //JSON.Serialize(value, streamWriter, _jilOptions);

                return Task.FromResult(writeStream);
            }
        }
    }
}