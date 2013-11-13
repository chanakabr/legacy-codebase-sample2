using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TVPWebApi.Models
{
    public class ParitalResponseContractResolver : DefaultContractResolver
    {

        public List<string> _fields = null;

        private List<Type> _types = new List<Type>();

        public ParitalResponseContractResolver(string fields)
        {
            _fields = fields.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            if (_fields != null && _fields.Count > 0)
            {
                if (!_types.Contains(type))
                {
                    properties = properties.Where(p => _fields.Contains(p.PropertyName)).ToList();

                    foreach (JsonProperty property in properties)
                    {
                        Type jpType = property.PropertyType;

                        if (jpType.IsGenericType)
                        {
                            Type genericType = jpType.GenericTypeArguments[0];

                            IList<JsonProperty> genericTypeProperties = base.CreateProperties(genericType, memberSerialization);

                            foreach (JsonProperty genericTypeProperty in genericTypeProperties)
                            {
                                _fields.Add(genericTypeProperty.PropertyName);
                            }
                        }
                    }
                }
                    
                foreach (JsonProperty property in properties)
                {
                    _types.Add(property.PropertyType);
                }
            }
            
            return properties;
        }
    }
}