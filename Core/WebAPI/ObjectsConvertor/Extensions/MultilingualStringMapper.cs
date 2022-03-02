using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Models.General;
using WebAPI.ModelsFactory;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class MultilingualStringMapper
    {
        public static List<LanguageContainer> GetLanugageContainer(this KalturaMultilingualString model)
        {
            List<LanguageContainer> languageContainer = new List<LanguageContainer>();
            if (model.Values != null && model.Values.Count > 0)
            {
                foreach (KalturaTranslationToken token in model.Values)
                {
                    LanguageContainer lng = new LanguageContainer(token.Language, token.Value);
                    languageContainer.Add(lng);
                }
            }

            return languageContainer;
        }

        public static List<LanguageContainer> GetNoneDefaultLanugageContainer(this KalturaMultilingualString model)
        {
            List<LanguageContainer> languageContainer = null;
            if (model.Values != null)
            {
                languageContainer = new List<LanguageContainer>();
                var GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();

                foreach (KalturaTranslationToken token in model.Values)
                {
                    if (token.Language != GroupDefaultLanguageCode)
                    {
                        LanguageContainer lng = new LanguageContainer(token.Language, token.Value);
                        languageContainer.Add(lng);
                    }
                }
            }

            return languageContainer;
        }

        public static string GetDefaultLanugageValue(this KalturaMultilingualString model)
        {
            var GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
            if (model.Values != null)
            {
                KalturaTranslationToken token = model.Values.FirstOrDefault(x => x.Language == GroupDefaultLanguageCode);
                if (token != null)
                {
                    return token.Value;
                }
            }

            return null;
        }

        public static string GetCurrent(LanguageContainer[] values, string value)
        {
            if (values != null)
            {
                return MultilengualStringFactory.Create(values).ToString();
            }

            return value;
        }

        public static string GetMultilingualName(string name)
        {
            return string.Format("multilingual{0}{1}", name.Substring(0, 1).ToUpper(), name.Substring(1)); ;
        }

        public static string ToString(KalturaMultilingualString model)
        {
            if (model.Values != null && model.Values.Count > 0)
            {
                var RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
                KalturaTranslationToken token = model.Values.FirstOrDefault(translation => translation.Language.Equals(RequestLanguageCode));
                if (token != null)
                {
                    return token.Value;
                }

                var GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
                if (GroupDefaultLanguageCode != null && !GroupDefaultLanguageCode.Equals(RequestLanguageCode))
                {
                    token = model.Values.FirstOrDefault(translation => translation.Language.Equals(GroupDefaultLanguageCode));
                    if (token != null)
                    {
                        return token.Value;
                    }
                }
            }

            return null;
        }

        public static string ToCustomJson(this KalturaMultilingualString model, Version currentVersion, bool omitObsolete, string propertyName)
        {
            string ret = null;
            string value = MultilingualStringMapper.ToString(model);
            if (value != null)
            {
                ret = "\"" + propertyName + "\": \"" + model.EscapeJson(value) + "\"";
            }

            string language = Utils.Utils.GetLanguageFromRequest();
            if (model.Values != null && language != null && language.Equals("*"))
            {
                string multilingualName = MultilingualStringMapper.GetMultilingualName(propertyName);
                if (ret != null)
                {
                    ret += ", ";
                }
                ret += "\"" + multilingualName + "\": [" + String.Join(", ", model.Values.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
            }
            return ret;
        }

        public static string ToCustomXml(this KalturaMultilingualString model, Version currentVersion, bool omitObsolete, string propertyName)
        {
            string ret = "";
            string value = MultilingualStringMapper.ToString(model);
            if (value != null)
            {
                ret = "<" + propertyName + ">" + model.EscapeXml(value) + "</" + propertyName + ">";
            }

            string language = Utils.Utils.GetLanguageFromRequest();
            if (model.Values != null && language != null && language.Equals("*"))
            {
                string multilingualName = MultilingualStringMapper.GetMultilingualName(propertyName);
                ret += "<" + multilingualName + "><item>" + String.Join("</item><item>", model.Values.Select(item => item.ToXml(currentVersion, omitObsolete))) + "</item></" + multilingualName + ">";
            }
            return ret;
        }
    }
}