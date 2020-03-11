using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.SearchObjects
{
    [JsonObject()]
    public class TagValue
    {
        [JsonProperty("tag_id")]
        public long tagId;

        [JsonProperty("topic_id")]
        public int topicId;

        [JsonProperty("language_id")]
        public int languageId;

        [JsonProperty("value")]
        public string value;

        [JsonProperty(PropertyName = "create_date")]
        public long createDate;

        [JsonProperty(PropertyName = "update_date")]
        public long updateDate;

        public List<LanguageContainer> TagsInOtherLanguages { get; set; }

        public TagValue()
        {
            this.TagsInOtherLanguages = new List<LanguageContainer>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("tagId: {0}, ", tagId));
            sb.AppendFormat("topicId: {0}, ", topicId);
            sb.AppendFormat("languageId: {0}, ", languageId);
            sb.AppendFormat("value: {0}, ", value);
            sb.AppendFormat("createDate: {0}, ", createDate);
            sb.AppendFormat("updateDate: {0}.", updateDate);
            if (TagsInOtherLanguages != null && TagsInOtherLanguages.Count > 0)
            {
                sb.AppendLine("TagsInOtherLanguages:");
                foreach (var tagInOtherLanguages in TagsInOtherLanguages)
                {
                    sb.AppendFormat("Tag: {0}.", tagInOtherLanguages.ToString());
                }
            }
            return sb.ToString();
        }
        
        public bool IsNeedToUpdate(TagValue other)
        {
            if (other == null)
                return false;

            if (this.tagId != other.tagId)
                return false;

            if (this.topicId != other.topicId || !this.value.ToLower().Equals(other.value.ToLower()))
                return true;
            
            if (other.TagsInOtherLanguages != null && other.TagsInOtherLanguages.Count > 0)
            {
                if (this.TagsInOtherLanguages == null || this.TagsInOtherLanguages.Count < other.TagsInOtherLanguages.Count)
                    return true;

                Dictionary<string, string> otherTagsInOtherLanguages = other.TagsInOtherLanguages.ToDictionary(x => x.m_sLanguageCode3, x => x.m_sValue.ToLower());

                foreach (var currTagInOtherLang in this.TagsInOtherLanguages)
                {
                   if (otherTagsInOtherLanguages.ContainsKey(currTagInOtherLang.m_sLanguageCode3) &&
                       !otherTagsInOtherLanguages[currTagInOtherLang.m_sLanguageCode3].Equals(currTagInOtherLang.m_sValue.ToLower()))
                        return true;
                }
            }

            return false;
        }
    }
}
