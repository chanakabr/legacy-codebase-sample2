using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPVR.AlcatelLucentResponses
{
    [Serializable]
    public class ReadResponseJSON
    {
        [JsonProperty("entries")]
        public List<EntryJSON> entries;

        [JsonProperty("entriesLength")]
        public int EntriesLength { get; set; }

        [JsonProperty("entriesStartIndex")]
        public int EntriesStartIndex { get; set; }

        [JsonProperty("entriesPageSize")]
        public int EntriesPageSize { get; set; }
    }
}
