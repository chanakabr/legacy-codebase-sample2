using ApiObjects.CrowdsourceItems.Base;
using Newtonsoft.Json;

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

        public override eItemType Type
        {
            get { return eItemType.LinearViews; }
        }
    }
}
