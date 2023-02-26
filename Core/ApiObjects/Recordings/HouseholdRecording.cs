using System;

namespace ApiObjects.Recordings
{
    public class HouseholdRecording
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long HouseholdId { get; set; }
        public long EpgId { get; set; }
        public string RecordingKey { get; set; }
        public string Status { get; set; }
        public string RecordingType { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public long ProtectedUntilEpoch { get; set; }
        public long EpgChannelId { get; set; }
        public bool ScheduledSaved { get; set; }
        public bool IsStopped { get; set; }
        
        public DateTime __updated { get; set; }
        
        public HouseholdRecording(long userId, long householdId, long epgId, string recordingKey, string status, 
            string recordingType, DateTime createDate, DateTime updateDate, long? protectedUntilEpoch, long epgChannelId, 
            bool scheduledSaved, bool isStopped = false)
        {
            UserId = userId;
            HouseholdId = householdId;
            EpgId = epgId;
            RecordingKey = recordingKey;
            Status = status;
            RecordingType = recordingType;
            CreateDate = createDate;
            UpdateDate = updateDate;

            if (protectedUntilEpoch.HasValue)
            {
                ProtectedUntilEpoch = protectedUntilEpoch.Value;
            }

            EpgChannelId = epgChannelId;
            ScheduledSaved = scheduledSaved;
            IsStopped = isStopped;
        }
    }
}