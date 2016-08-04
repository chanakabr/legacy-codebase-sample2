using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    [Serializable]
    [JsonObject(Id = "epg")]
    public class RecordingCB : EpgCB
    {
        [JsonProperty("recording_id")]
        public ulong RecordingId;

        public RecordingCB(EpgCB baseEpg)
        {
            this.BasicData = baseEpg.BasicData;
            this.ChannelID = baseEpg.ChannelID;
            this.CoGuid = baseEpg.CoGuid;
            this.CreateDate = baseEpg.CreateDate;
            this.Crid = baseEpg.Crid;
            this.Description = baseEpg.Description;
            this.EnableCatchUp = baseEpg.EnableCatchUp;
            this.EnableCDVR = baseEpg.EnableCDVR;
            this.EnableStartOver = baseEpg.EnableStartOver;
            this.EnableTrickPlay = baseEpg.EnableTrickPlay;
            this.EndDate = baseEpg.EndDate;
            this.EpgID = baseEpg.EpgID;
            this.EpgIdentifier = baseEpg.EpgIdentifier;
            this.ExtraData = baseEpg.ExtraData;
            this.GroupID = baseEpg.GroupID;
            this.isActive = baseEpg.isActive;
            this.Language = baseEpg.Language;
            this.Metas = new Dictionary<string, List<string>>(baseEpg.Metas);
            this.Name = baseEpg.Name;
            this.ParentGroupID = baseEpg.ParentGroupID;
            this.PicID = baseEpg.PicID;
            this.pictures = new List<Epg.EpgPicture>(baseEpg.pictures);
            this.PicUrl = baseEpg.PicUrl;
            this.SearchEndDate = baseEpg.SearchEndDate;
            this.StartDate = baseEpg.StartDate;
            this.Statistics = baseEpg.Statistics;
            this.Status = baseEpg.Status;
            this.Tags = new Dictionary<string, List<string>>(baseEpg.Tags);
            this.Type = baseEpg.Type;
            this.UpdateDate = baseEpg.UpdateDate;
        }
    }
}
