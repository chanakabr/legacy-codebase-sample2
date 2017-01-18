using ApiObjects.CrowdsourceItems.Base;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ApiObjects.CrowdsourceItems.Implementations
{
    public class RealTimeViewsItem : BaseCrowdsourceItem
    {
        [JsonProperty("programId")]
        public long ProgramId { get; set; }
        [JsonProperty("programName")]
        public string ProgramName { get; set; }
        [JsonProperty("programImage")]
        public string ProgramImage { get; set; }
        [JsonProperty("Views")]
        public int Views { get; set; }
        [JsonProperty("epgStartTime")]
        public long EpgStartTime { get; set; }

        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public override eItemType Type
        {
            get { return eItemType.LinearViews; }
        }

        [JsonProperty("typeDescription")]
        public string TypeDescription
        {
            get { return eItemType.LinearViews.ToString(); }
            set { }
        }
    }
}
