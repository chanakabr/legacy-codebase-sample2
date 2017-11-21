using ApiObjects.ConditionalAccess;
using Core.ConditionalAccess;
using NPVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ConditionalAccess
{
    public class DeleteSeriesRecordingByNPVRCommand : BaseNPVRCommand
    {
        public string SeriesId { get; set; }

        public int SeasonNumber { get; set; }

        public List<NPVRRecordingStatus> Status { get; set; }

        public int ChannelId { get; set; }
        

        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.DeleteNPVR(siteGuid, SeriesId, SeasonNumber, ChannelId, Status);
        }
    }   
}
