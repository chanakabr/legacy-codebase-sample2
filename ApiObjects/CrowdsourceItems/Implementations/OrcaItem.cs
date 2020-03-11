using ApiObjects.CrowdsourceItems.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ApiObjects.CrowdsourceItems.Implementations
{
    public class OrcaItem : BaseCrowdsourceItem
    {
        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public override eItemType Type
        {
            get { return eItemType.Recommendation; }
        }

        [JsonProperty("typeDescription")]
        public string TypeDescription
        {
            get { return eItemType.Recommendation.ToString(); }
        }
    }
}
