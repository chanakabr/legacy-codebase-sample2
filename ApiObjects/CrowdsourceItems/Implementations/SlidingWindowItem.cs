using ApiObjects.CrowdsourceItems.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ApiObjects.CrowdsourceItems.Implementations
{
    public class SlidingWindowItem : BaseCrowdsourceItem
    {
        [JsonProperty("action"), JsonConverter(typeof(StringEnumConverter))]
        public ApiObjects.SearchObjects.OrderBy Action { get; set; }
        [JsonProperty("actionDescription")]
        public string ActionDescription { get; set; }
        [JsonProperty("actionVal")]
        public double ActionVal { get; set; }
        [JsonProperty("period")]
        public int Period { get; set; }
        [JsonProperty("periodDescription")]
        public string PeriodDescription { get; set; }

        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public override eItemType Type
        {
            get { return eItemType.VOD; }
        }

        [JsonProperty("typeDescription")]
        public string TypeDescription { 
            get { return eItemType.VOD.ToString(); }
            set { }
        }
    }
}
