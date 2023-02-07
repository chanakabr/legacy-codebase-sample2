using System;
using ApiObjects.TimeShiftedTv;

namespace ApiObjects.Recordings
{
    public class TimeBasedRecording
    {
        public long Id { get; set; }
        public string Key { get; set; }
        public long EpgId { get; set; }
        public long EpgChannelId { get; set; }
        public long ProgramId { get; set; }
        public string ExternalId { get; set; }
        public string Status { get; set; }
        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        // fined name - StatusCount?
        public int RetriesStatus { get; set; }

        public long ViewableUntilEpoch { get; set; }

        // bool?
        public bool LifeTimeExpiryHandled { get; set; }
        public string Crid { get; set; }
        public int PaddingBeforeMins { get; set; }
        public int PaddingAfterMins { get; set; }

        public DateTime? AbsoluteStartTime { get; set; }
        public DateTime? AbsoluteEndTime { get; set; }

        public DateTime __updated { get; set; }

        public TimeBasedRecording(string key, long epgId, long epgChannelId, long programId, string status,
            DateTime createDate, DateTime updateDate, DateTime viewableUntilDate, long viewableUntilEpoch,
            bool lifeTimeExpiryHandled, string crid, int paddingBeforeMins, int paddingAfterMins)
        {
            Key = key;
            EpgId = epgId;
            EpgChannelId = epgChannelId;
            ProgramId = programId;
            Status = status;
            CreateDate = createDate;
            UpdateDate = updateDate;
            ViewableUntilEpoch = viewableUntilEpoch;
            LifeTimeExpiryHandled = lifeTimeExpiryHandled;
            Crid = crid;
            PaddingBeforeMins = paddingBeforeMins;
            PaddingAfterMins = paddingAfterMins;
        }

        public TimeBasedRecording(string key, long epgChannelId, long programId, bool lifeTimeExpiryHandled, Recording recording)
        {
            Key = key;
            EpgId = recording.EpgId;
            EpgChannelId = epgChannelId;
            ProgramId =  programId;
            Status = recording.Status.ToString();
            CreateDate = recording.CreateDate;
            UpdateDate = recording.UpdateDate;
            ViewableUntilEpoch = recording.ViewableUntilDate ?? 0;
            LifeTimeExpiryHandled = lifeTimeExpiryHandled;
            Crid = recording.Crid;
            PaddingBeforeMins = recording.StartPadding ?? 0;
            PaddingAfterMins = recording.EndPadding ?? 0;
        }

        public void SetStatus(TstvRecordingStatus tstvRecordingStatus)
        {
            Status = tstvRecordingStatus.ToString();
        }

        public bool IsImmediateRecording() => AbsoluteEndTime.HasValue;
    }
}
  