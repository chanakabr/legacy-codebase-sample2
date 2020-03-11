using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.Epg
{
    [JsonObject(Title = "Epg")]
    public class EpgObject
    {
        [JsonProperty("ChannelId")]
        public string ChannelId { get; set; }

        [JsonProperty("GroupID")]
        public int GroupID { get; set; }

        [JsonProperty("ParentGroupID")]
        public int ParentGroupID { get; set; }

        [JsonProperty("MainLangu")]
        public string MainLangu { get; set; }

        [JsonProperty("UpdaterID")]
        public int UpdaterID { get; set; }

        [JsonProperty("lProgramObject")]
        public List<ProgramObject> lProgramObject { get; set; }

    }
}
