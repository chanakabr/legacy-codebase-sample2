using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TVPApi.Common
{
    public class TvpapiCustomContractResolver : DefaultContractResolver
    {

        private static Dictionary<Type, Dictionary<string, string>> objectPropertiesRenames = null;
        private static object locker = new object();
        private static TvpapiCustomContractResolver instance = null;

        public TvpapiCustomContractResolver()
        {
            objectPropertiesRenames = GetAllPropertiesForRename();
        }

        public static TvpapiCustomContractResolver Instance
        {
            get
            {
                if (instance == null || objectPropertiesRenames == null)
                {
                    lock (locker)
                    {
                        if (instance == null || objectPropertiesRenames == null)
                        {
                            instance = new TvpapiCustomContractResolver();
                        }
                    }
                }

                return instance;
            }
        }

        // dictionary which contains all types and their properties to rename, for example "<LanguageContainer, <"LanguageCode", "m_sLanguageCode3">>
        private Dictionary<Type, Dictionary<string, string>> GetAllPropertiesForRename()
        {
            Dictionary<Type, Dictionary<string, string>> objectTypesToRenamedPropertiesMap = new Dictionary<Type, Dictionary<string, string>>();
            AddTypePropertyMap(objectTypesToRenamedPropertiesMap, typeof(ApiObjects.LanguageContainer), new KeyValuePair<string, string>("LanguageCode", "m_sLanguageCode3"));
            AddTypePropertyMap(objectTypesToRenamedPropertiesMap, typeof(ApiObjects.LanguageContainer), new KeyValuePair<string, string>("Value", "m_sValue"));

            return objectTypesToRenamedPropertiesMap;

        }

        private void AddTypePropertyMap(Dictionary<Type, Dictionary<string, string>> objectTypesToRenamedPropertiesMap, Type type, KeyValuePair<string, string> pair)
        {
            if (!objectTypesToRenamedPropertiesMap.ContainsKey(type))
            {
                objectTypesToRenamedPropertiesMap.Add(type, new Dictionary<string, string>());
            }

            if (!objectTypesToRenamedPropertiesMap[type].ContainsKey(pair.Key))
            {
                objectTypesToRenamedPropertiesMap[type].Add(pair.Key, pair.Value);
            }
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (objectPropertiesRenames.ContainsKey(property.DeclaringType) && objectPropertiesRenames[property.DeclaringType].ContainsKey(property.PropertyName))
            {
                property.PropertyName = objectPropertiesRenames[property.DeclaringType][property.PropertyName];
            }

            return property;
        }


    }
}
