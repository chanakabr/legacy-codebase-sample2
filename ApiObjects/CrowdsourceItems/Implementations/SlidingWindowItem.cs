using ApiObjects.CrowdsourceItems.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ApiObjects.CrowdsourceItems.Implementations
{
    public class SlidingWindowItem : BaseCrowdsourceItem
    {
        [JsonProperty("action"), JsonConverter(typeof(StringEnumConverter))]
        public ApiObjects.SearchObjects.OrderBy Action { get; set; }
        [JsonProperty("actionVal")]
        public int ActionVal { get; set; }

        public override eItemType Type
        {
            get { return eItemType.VOD; }
        }
    }
}
