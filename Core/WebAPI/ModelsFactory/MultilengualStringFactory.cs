using ApiObjects;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.ModelsFactory
{
    public class MultilengualStringFactory
    {
        public static KalturaMultilingualString Create(LanguageContainer[] values)
        {
            var multilingualString = new KalturaMultilingualString(null);
            if (values == null || values.Length == 0)
            {
                var GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
                List<LanguageContainer> tempValuesList = new List<LanguageContainer>();
                tempValuesList.Add(new LanguageContainer(GroupDefaultLanguageCode, string.Empty, true));
                values = tempValuesList.ToArray();
            }

            multilingualString.Values = AutoMapper.Mapper.Map<List<KalturaTranslationToken>>(values);
            return multilingualString;
        }

        public static KalturaMultilingualString Create(List<LanguageContainer> values, string defaultLanguageValue)
        {
            var multilingualString = new KalturaMultilingualString(null);
            var RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
            var GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();

            List<LanguageContainer> tempValuesList = values != null ? new List<LanguageContainer>(values) : new List<LanguageContainer>();

            if (!string.IsNullOrEmpty(GroupDefaultLanguageCode) &&
                !tempValuesList.Any(x => x.m_sLanguageCode3 == GroupDefaultLanguageCode))
            {
                tempValuesList.Add(new LanguageContainer(GroupDefaultLanguageCode, defaultLanguageValue, true));
            }

            multilingualString.Values = AutoMapper.Mapper.Map<List<KalturaTranslationToken>>(tempValuesList);
            return multilingualString;
        }

        public static KalturaMultilingualString Create(JArray values)
        {
            var multilingualString = new KalturaMultilingualString(null);
            multilingualString.Values = OTTObjectBuilder.buildList<KalturaTranslationToken>(typeof(KalturaTranslationToken), values);
            return multilingualString;
        }

        public static KalturaMultilingualString Create(List<object> values)
        {
            var multilingualString = new KalturaMultilingualString(null);
            multilingualString.Values = OTTObjectBuilder.buildList(typeof(KalturaTranslationToken), values.ToArray());
            return multilingualString;
        }

        public static KalturaMultilingualString Create(string defaultLanguageValue)
        {
            var multilingualString = new KalturaMultilingualString(null);
            var GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
            List<LanguageContainer> tempValuesList = new List<LanguageContainer>();
            tempValuesList.Add(new LanguageContainer(GroupDefaultLanguageCode, defaultLanguageValue, true));
            multilingualString.Values = AutoMapper.Mapper.Map<List<KalturaTranslationToken>>(tempValuesList);
            return multilingualString;
        }
    }
}
