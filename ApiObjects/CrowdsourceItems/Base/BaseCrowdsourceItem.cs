using ApiObjects.CrowdsourceItems.Implementations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace ApiObjects.CrowdsourceItems.Base
{
    [KnownType(typeof(OrcaItem))]
    [KnownType(typeof(SlidingWindowItem))]
    [KnownType(typeof(RealTimeViewsItem))]
    public abstract class BaseCrowdsourceItem
    {
        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public virtual eItemType Type
        {
            get { return eItemType.LinearViews; }
            set {}
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
