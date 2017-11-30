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
        [JsonProperty("tag_value")]
        public string value;
        [JsonProperty(PropertyName = "create_date")]
        public long createDate;
        [JsonProperty(PropertyName = "update_date")]
        public long updateDate;
    }
}
