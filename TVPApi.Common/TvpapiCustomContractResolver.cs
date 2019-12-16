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
            objectTypesToRenamedPropertiesMap.Add(typeof(ApiObjects.LanguageContainer), new Dictionary<string, string>() { { "LanguageCode", "m_sLanguageCode3" } });
            objectTypesToRenamedPropertiesMap.Add(typeof(ApiObjects.LanguageContainer), new Dictionary<string, string>() { { "Value", "m_sValue" } });

            return objectTypesToRenamedPropertiesMap;

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
