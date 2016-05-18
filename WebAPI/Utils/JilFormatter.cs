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
using WebAPI.Managers.Schema;
using System.Dynamic;
using Newtonsoft.Json;

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
                if (value == null)
                    continue;

                string name = getApiName(property);

                if (property.PropertyType.IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    if (OldStandardAttribute.getOldMembers(value.GetType()) != null)
                        value = new OldStandardObject((KalturaOTTObject)value);
                }
                else if (property.PropertyType.IsArray || property.PropertyType.IsGenericType)
                {
                    if (property.PropertyType.GetGenericArguments().Count() == 1 && (property.PropertyType.GetGenericArguments()[0].IsSubclassOf(typeof(KalturaOTTObject))))
                    {
                        var array = new List<OldStandardObject>();
                        foreach(KalturaOTTObject item in (IEnumerable<KalturaOTTObject>)value)
                        {
                            if (OldStandardAttribute.getOldMembers(item.GetType()) == null)
                            {
                                break;
                            }
                            array.Add(new OldStandardObject(item));
                        }
                        if (array.Count > 0)
                            value = array;
                    }
                    else if (property.PropertyType.GetGenericArguments().Count() == 2 && property.PropertyType.GetGenericArguments()[1].IsSubclassOf(typeof(KalturaOTTObject)))
                    {
                        var dictionary = new Dictionary<string, OldStandardObject>();
                        foreach (KeyValuePair<string, KalturaOTTObject> item in (dynamic) value)
                        {
                            if (OldStandardAttribute.getOldMembers(item.Value.GetType()) == null)
                            {
                                break;
                            }
                            dictionary.Add(item.Key, new OldStandardObject(item.Value));
                        }
                        if (dictionary.Count > 0)
                            value = dictionary;
                    }
                }

                _fields[name] = value;
                if (oldStandardProperties.ContainsKey(name))
                {
                    _fields[oldStandardProperties[name]] = value;
                }
            }
        }

        private string getApiName(PropertyInfo property)
        {
            System.Runtime.Serialization.DataMemberAttribute dataMember = property.GetCustomAttribute<System.Runtime.Serialization.DataMemberAttribute>();
            if (dataMember == null)
                return null;

            return dataMember.Name;
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            var membersNames = GetType().GetProperties().Where(propInfo => propInfo.CustomAttributes
                .All(ca => ca.AttributeType != typeof(JsonIgnoreAttribute)))
                .Select(propInfo => propInfo.Name);
            return Extra.Keys.Union(membersNames);
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

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
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
                throw new BadRequestException((int)StatusCode.BadRequest, "One or more parameters have invalid structure");
            }
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            using (TextWriter streamWriter = new StreamWriter(writeStream))
            {
                if (type == typeof(StatusWrapper) && ((StatusWrapper)value).Result.GetType().IsSubclassOf(typeof(KalturaOTTObject)))
                {
                    StatusWrapper statusWrapper = ((StatusWrapper)value);
                    KalturaOTTObject result = (KalturaOTTObject)statusWrapper.Result;
                    Dictionary<string, string> oldStandardProperties = OldStandardAttribute.getOldMembers(result.GetType());
                    if (oldStandardProperties != null)
                    {
                        dynamic oldStandardObject = new OldStandardObject((KalturaOTTObject)result);
                        statusWrapper.Result = oldStandardObject;
                    }
                }
                JSON.Serialize(value, streamWriter, _jilOptions);
                return Task.FromResult(writeStream);
            }
        }
    }
}