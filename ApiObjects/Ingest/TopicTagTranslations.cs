using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiObjects
{
    public class TagsTranslations
    {
        public string TopicSystemName { get; set; }
        public long TopicId { get; set; }
        public string DefaultTagValue { get; set; }
        public long TagId { get; set; }

        // asset id and if asset is exists
        public List<KeyValuePair<int, bool>> AssetsToInvalidate { get; set; }
        public Dictionary<string, LanguageContainer> Translations { get; set; }
        
        public TagsTranslations(string topicSystemName, string defaultTagValue, LanguageContainer[] translations, int mediaId, bool isMediaExists)
        {
            this.TopicSystemName = topicSystemName;
            this.DefaultTagValue = defaultTagValue;
            if (translations == null)
            {
                this.Translations = new Dictionary<string, LanguageContainer>();
            }
            else
            {
                this.Translations = translations.ToDictionary(x => x.m_sLanguageCode3, y => y);
            }
            
            this.AssetsToInvalidate = new List<KeyValuePair<int, bool>>() { new KeyValuePair<int, bool>(mediaId, isMediaExists) } ;
        }

        public TagToInvalidate GetTagToInvalidate(bool isTagExists, int defaultLanguageId)
        {
            var tagToInvalidate = new TagToInvalidate()
            {
                AssetsToInvalidate = this.AssetsToInvalidate,
                IsTagExists = isTagExists,
                TagValue = new TagValue()
                {
                    topicId = (int)this.TopicId, // genre
                    tagId = this.TagId,
                    value = this.DefaultTagValue, // eng_drama
                    languageId = defaultLanguageId, // eng
                    TagsInOtherLanguages = new List<LanguageContainer>(this.Translations.Values.Select(x => x)) // drama in all other lang
                }
            };

            return tagToInvalidate;
        }
        
        public static string GetKey(string topicSystemName, string defaultTagValue)
        {
            return string.Format("{0}_{1}", topicSystemName, defaultTagValue);
        }
    }
    
    public class TagToInvalidate
    {
        public bool IsTagExists { get; set; }
        public List<KeyValuePair<int, bool>> AssetsToInvalidate { get; set; }
        public TagValue TagValue { get; set; }
    }
}
