using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public class DeleteNPVRAllRecordingsCommand : BaseNPVRCommand
    {
        public bool? DeleteProtected { get; set; }
        public bool? DeleteBookings { get; set; }

        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.DeleteAllRecordings(siteGuid, Version, DeleteProtected, DeleteBookings);
        }
    }

    public class CancelNPVRByRecordingsCommand : BaseNPVRCommand
    {
        public string ByChannelId { get; set; }
        public string ByAssetId { get; set; }
        public string BySeriesId { get; set; }
        public string BySeasonNumber { get; set; }
        public string ByAlreadyWatched { get; set; }
        public string ByProgramId { get; set; }
        public bool? DeleteOngoingRecordings { get; set; }
       
        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.CancelByRecording(siteGuid, Version, ByChannelId, ByAssetId, BySeriesId, BySeasonNumber, ByAlreadyWatched, ByProgramId, DeleteOngoingRecordings);
        }
    }
}
