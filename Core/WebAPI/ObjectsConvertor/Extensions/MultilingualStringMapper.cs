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
                if (string.IsNullOrEmpty(model.GroupDefaultLanguageCode))
                {
                    model.GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
                }

                foreach (KalturaTranslationToken token in model.Values)
                {
                    if (token.Language != model.GroupDefaultLanguageCode)
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
            if (string.IsNullOrEmpty(model.GroupDefaultLanguageCode))
            {
                model.GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
            }

            if (model.Values != null)
            {
                KalturaTranslationToken token = model.Values.FirstOrDefault(x => x.Language == model.GroupDefaultLanguageCode);
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

        public static string ToString(this KalturaMultilingualString model)
        {
            if (model.Values != null && model.Values.Count > 0)
            {
                KalturaTranslationToken token = model.Values.FirstOrDefault(translation => translation.Language.Equals(model.RequestLanguageCode));
                if (token != null)
                {
                    return token.Value;
                }

                if (model.GroupDefaultLanguageCode != null && !model.GroupDefaultLanguageCode.Equals(model.RequestLanguageCode))
                {
                    token = model.Values.FirstOrDefault(translation => translation.Language.Equals(model.GroupDefaultLanguageCode));
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
            string value = model.ToString();
            if (value != null)
            {
                ret = "\"" + propertyName + "\": \"" + model.EscapeJson(model.ToString()) + "\"";
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
            string value = model.ToString();
            if (value != null)
            {
                ret = "<" + propertyName + ">" + model.EscapeXml(model.ToString()) + "</" + propertyName + ">";
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