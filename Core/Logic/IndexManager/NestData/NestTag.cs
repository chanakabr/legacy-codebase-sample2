using System.Collections.Generic;
using ApiObjects.SearchObjects;
using Nest;

namespace ApiLogic.IndexManager.NestData
{
    public class NestTag
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

        public NestTag(TagValue tagValue, string languageCode)
        {
            tagId = tagValue.tagId;
            topicId = tagValue.topicId;
            languageId = tagValue.languageId;
            value = new Dictionary<string, string>
            {
                { languageCode, tagValue.value }
            };
            createDate = tagValue.createDate;
            updateDate = tagValue.updateDate;
        }

        public NestTag(long tagId, int topicId, int languageId, string value, string languageCode, long createDate, long updateDate)
        {
            this.tagId = tagId;
            this.topicId = topicId;
            this.languageId = languageId;
            this.value = new Dictionary<string, string>()
            {
                { languageCode, value }
            };
            this.createDate = createDate;
            this.updateDate = updateDate;
        }
    }
}
