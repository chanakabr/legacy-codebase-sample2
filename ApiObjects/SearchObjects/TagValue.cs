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
        public int tagId;
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("tagId: {0}, ", tagId));
            sb.AppendFormat("topicId: {0},", topicId);
            sb.AppendFormat("languageId: {0}, ", languageId);
            sb.AppendFormat("value: {0}, ", value);
            sb.AppendFormat("createDate: {0}, ", createDate);
            sb.AppendFormat("updateDate: {0}", updateDate);
            return sb.ToString();
        }
    }
}
