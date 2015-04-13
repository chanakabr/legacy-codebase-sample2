using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Epg
{
    [Serializable]
    [JsonObject(Title = "EpgDictionary")]
    public class EpgDictionary
    {
        [JsonProperty("channelID")]
        public int channelID { get; set; }
        [JsonProperty("groupID")]
        public int groupID { get; set; }
        [JsonProperty("parentGroupID")]
        public int parentGroupID { get; set; }
        [JsonProperty("epgs")]
        public Dictionary<string, Dictionary<string, EpgCB>> epgs { get; set; }

        public List<DateTime> deletedDays { get; set; }

        public EpgDictionary()
        {
            epgs = new Dictionary<string, Dictionary<string, EpgCB>>();
            deletedDays = new List<DateTime>();
        }

        public EpgDictionary(int channelID, Dictionary<string, Dictionary<string, EpgCB>> epgs, int groupID, int parentGroupID, List<DateTime> deletedDays)
        {
            this.channelID = channelID;
            this.epgs = epgs;
            this.groupID = groupID;
            this.parentGroupID = parentGroupID;
            this.deletedDays = deletedDays;
        }
    }
}
