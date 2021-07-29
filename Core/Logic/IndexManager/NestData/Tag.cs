using ApiObjects.SearchObjects;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.NestData
{
    public class Tag
    {
        [PropertyName("tag_id")]
        public long tagId { get; set; }

        [PropertyName("topic_id")]
        public int topicId { get; set; }

        [PropertyName("language_id")]
        public int languageId { get; set; }

        [PropertyName("value")]
        public Dictionary<string, string> value { get; set; }

        [PropertyName("create_date")]
        public long createDate { get; set; }

        [PropertyName("update_date")]
        public long updateDate { get; set; }

        public Tag(TagValue tagValue, string languageCode)
        {
            this.tagId = tagValue.tagId;
            this.topicId = tagValue.topicId;
            this.languageId = tagValue.languageId;
            this.value = new Dictionary<string, string>()
            {
                { languageCode, tagValue.value }
            };
            this.createDate = tagValue.createDate;
            this.updateDate = tagValue.updateDate;
        }
    }
}
