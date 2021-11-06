using ApiObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Models.General;

namespace WebAPI.ModelsFactory
{
    public static class MultilengualStringFactory
    {
        public static KalturaMultilingualString Create(LanguageContainer[] values)
        {
            var multilingualString = new KalturaMultilingualString(null);
            multilingualString.RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
            multilingualString.GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
            if (values == null || values.Length == 0)
            {
                List<LanguageContainer> tempValuesList = new List<LanguageContainer>();
                tempValuesList.Add(new LanguageContainer(multilingualString.GroupDefaultLanguageCode, string.Empty, true));
                values = tempValuesList.ToArray();
            }

            multilingualString.Values = AutoMapper.Mapper.Map<List<KalturaTranslationToken>>(values);
            return multilingualString;
        }

        public static KalturaMultilingualString Create(List<LanguageContainer> values, string defaultLanguageValue)
        {
            var multilingualString = new KalturaMultilingualString(null);
            multilingualString.RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
            multilingualString.GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();

            List<LanguageContainer> tempValuesList = values != null ? new List<LanguageContainer>(values) : new List<LanguageContainer>();

            if (!string.IsNullOrEmpty(multilingualString.GroupDefaultLanguageCode) &&
                !tempValuesList.Any(x => x.m_sLanguageCode3 == multilingualString.GroupDefaultLanguageCode))
            {
                tempValuesList.Add(new LanguageContainer(multilingualString.GroupDefaultLanguageCode, defaultLanguageValue, true));
            }

            multilingualString.Values = AutoMapper.Mapper.Map<List<KalturaTranslationToken>>(tempValuesList);
            return multilingualString;
        }

        public static KalturaMultilingualString Create(JArray values)
        {
            var multilingualString = new KalturaMultilingualString(null);
            multilingualString.RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
            multilingualString.GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
            multilingualString.Values = multilingualString.buildList<KalturaTranslationToken>(typeof(KalturaTranslationToken), values);
            return multilingualString;
        }

        public static KalturaMultilingualString Create(List<object> values)
        {
            var multilingualString = new KalturaMultilingualString(null);
            multilingualString.RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
            multilingualString.GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
            multilingualString.Values = KalturaOTTObject.buildList(typeof(KalturaTranslationToken), values.ToArray());
            return multilingualString;
        }

        public static KalturaMultilingualString Create(string defaultLanguageValue)
        {
            var multilingualString = new KalturaMultilingualString(null);
            multilingualString.RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
            multilingualString.GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
            List<LanguageContainer> tempValuesList = new List<LanguageContainer>();
            tempValuesList.Add(new LanguageContainer(multilingualString.GroupDefaultLanguageCode, defaultLanguageValue, true));
            multilingualString.Values = AutoMapper.Mapper.Map<List<KalturaTranslationToken>>(tempValuesList);
            return multilingualString;
        }
    }
}
