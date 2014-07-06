using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ApiObjects.CrowdsourceItems.Base
{
    public abstract class BaseCrowdsourceItem
    {
        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public virtual eItemType Type 
        {
            get { return eItemType.LinearViews; }
        }

        [JsonProperty("timstamp")]
        public long TimeStamp { get; set; }
        [JsonProperty("mediaId")]
        public int MediaId { get; set; }
        [JsonProperty("mediaName")]
        public string MediaName { get; set; }
        [JsonProperty("mediaImage")]
        public Pic[] MediaImage { get; set; }
        [JsonProperty("order")]
        public int Order { get; set; }

        public class Pic
        {
            public string Size { get; set; }
            public string URL { get; set; }
        }
    }
}
