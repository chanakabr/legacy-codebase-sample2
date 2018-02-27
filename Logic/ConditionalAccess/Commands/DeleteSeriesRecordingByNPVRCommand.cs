using ApiObjects.ConditionalAccess;
using NPVR;
using System.Collections.Generic;

namespace Core.ConditionalAccess
{
    public class DeleteSeriesRecordingByNPVRCommand : BaseNPVRCommand
    {
        public string SeriesId { get; set; }

        public string SeasonNumber { get; set; }

        public List<NPVRRecordingStatus> Status { get; set; }

        public string ChannelId { get; set; }        

        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.DeleteNPVR(siteGuid, SeriesId, SeasonNumber, ChannelId, Status, Version);
        }
    }   
}
