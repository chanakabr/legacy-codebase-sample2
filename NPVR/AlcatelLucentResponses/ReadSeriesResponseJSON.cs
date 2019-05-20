using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPVR.AlcatelLucentResponses
{
    [Serializable]
    public class ReadSeriesResponseJSON<T>
    {
        [JsonProperty("entriesLength")]
        public int EntriesLength { get; set; }

        [JsonProperty("entriesStartIndex")]
        public int EntriesStartIndex { get; set; }

        [JsonProperty("entriesPageSize")]
        public int EntriesPageSize { get; set; }

        [JsonProperty("entries")]
        public List<T> Entries { get; set; }

    }
}
