using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPVR.AlcatelLucentResponses
{
    [Serializable]
    public class RecordSeriesResponseJSON
    {
        [JsonProperty("id")]
        public string RecordingID;
    }
}
