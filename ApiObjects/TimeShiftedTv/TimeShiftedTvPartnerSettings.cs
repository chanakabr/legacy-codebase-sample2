using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class TimeShiftedTvPartnerSettings
    {

        public bool? IsCatchUpEnabled { get; set; }
        public bool? IsCdvrEnabled { get; set; }
        public bool? IsStartOverEnabled { get; set; }
        public bool? IsTrickPlayEnabled { get; set; }
        public bool? IsRecordingScheduleWindowEnabled { get; set; }
        public bool? IsProtectionEnabled { get; set; }
        public long? CatchUpBufferLength { get; set; }
        public long? TrickPlayBufferLength { get; set; }
        public long? RecordingScheduleWindow { get; set; }
        public long? PaddingAfterProgramEnds { get; set; }
        public long? PaddingBeforeProgramStarts { get; set; }
        public int? ProtectionPeriod { get; set; }
        public int? ProtectionQuotaPercentage { get; set; }
        public int? RecordingLifetimePeriod { get; set; }
        public int? CleanupNoticePeroid { get; set; }
        public bool? IsSeriesRecordingEnabled { get; set; }
        public bool? IsRecordingPlaybackNonEntitledChannelEnabled { get; set; }
        public bool? IsRecordingPlaybackNonExistingChannelEnabled { get; set; }

        public TimeShiftedTvPartnerSettings()
        {
        }

        public TimeShiftedTvPartnerSettings(bool? isCatchUpEnabled, bool? isCdvrEnabled, bool? isStartOverEnabled, bool? isTrickPlayEnabled, bool? isRecordingScheduleWindowEnabled,
            long? catchUpBufferLength, long? trickPlayBufferLength, long? recordingScheduleWindowBuffer, long? paddingAfterProgramEnds, long? paddingBeforeProgramStarts,
            bool? isProtectionEnabled, int? protectionPeriod, int? protectionQuotaPercentage, int? recordingLifetimePeroid, int? cleanupNoticePeroid, bool? isSeriesRecordingEnabled,
            bool? isRecordingPlaybackNonEntitledEnabled, bool? isRecordingPlaybackNonExistingEnabled)
        {
            this.IsCatchUpEnabled = isCatchUpEnabled;
            this.IsCdvrEnabled = isCdvrEnabled;
            this.IsStartOverEnabled = isStartOverEnabled;
            this.IsTrickPlayEnabled = isTrickPlayEnabled;
            this.CatchUpBufferLength = catchUpBufferLength;
            this.TrickPlayBufferLength = trickPlayBufferLength;
            this.RecordingScheduleWindow = recordingScheduleWindowBuffer;
            this.IsRecordingScheduleWindowEnabled = isRecordingScheduleWindowEnabled;
            this.PaddingAfterProgramEnds = paddingAfterProgramEnds;
            this.PaddingBeforeProgramStarts = paddingBeforeProgramStarts;
            this.IsProtectionEnabled = isProtectionEnabled;
            this.ProtectionPeriod = protectionPeriod;
            this.ProtectionQuotaPercentage = protectionQuotaPercentage;
            this.RecordingLifetimePeriod = recordingLifetimePeroid;
            this.CleanupNoticePeroid = cleanupNoticePeroid;
            this.IsSeriesRecordingEnabled = isSeriesRecordingEnabled;
            this.IsRecordingPlaybackNonEntitledChannelEnabled = isRecordingPlaybackNonEntitledEnabled;
            this.IsRecordingPlaybackNonExistingChannelEnabled = isRecordingPlaybackNonExistingEnabled;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("IsCatchUpEnabled: {0}, ", IsCatchUpEnabled.HasValue ? IsCatchUpEnabled.Value.ToString() : "Null"));
            sb.Append(string.Format("IsCdvrEnabled: {0}, ", IsCdvrEnabled.HasValue ? IsCdvrEnabled.Value.ToString() : "Null"));
            sb.Append(string.Format("IsStartOverEnabled: {0}, ", IsStartOverEnabled.HasValue ? IsStartOverEnabled.Value.ToString() : "Null"));
            sb.Append(string.Format("IsTrickPlayEnabled: {0}, ", IsTrickPlayEnabled.HasValue ? IsTrickPlayEnabled.Value.ToString() : "Null"));
            sb.Append(string.Format("IsRecordingScheduleWindowEnabled: {0}, ", IsRecordingScheduleWindowEnabled.HasValue ? IsRecordingScheduleWindowEnabled.Value.ToString() : "Null"));
            sb.Append(string.Format("CatchUpBufferLength: {0}, ", CatchUpBufferLength.HasValue ? CatchUpBufferLength.Value.ToString() : "Null"));
            sb.Append(string.Format("TrickPlayBufferLength: {0}, ", TrickPlayBufferLength.HasValue ? TrickPlayBufferLength.Value.ToString() : "Null"));
            sb.Append(string.Format("RecordingScheduleWindowBuffer: {0}, ", RecordingScheduleWindow.HasValue ? RecordingScheduleWindow.Value.ToString() : "Null"));
            sb.Append(string.Format("PaddingAfterRecord: {0}, ", PaddingAfterProgramEnds.HasValue ? PaddingAfterProgramEnds.Value.ToString() : "Null"));
            sb.Append(string.Format("PaddingBeforeRecord: {0}, ", PaddingBeforeProgramStarts.HasValue ? PaddingBeforeProgramStarts.Value.ToString() : "Null"));
            sb.Append(string.Format("IsProtectionEnabled: {0}, ", IsProtectionEnabled.HasValue ? IsProtectionEnabled.Value.ToString() : "Null"));
            sb.Append(string.Format("ProtectionPeriod: {0}, ", ProtectionPeriod.HasValue ? ProtectionPeriod.Value.ToString() : "Null"));
            sb.Append(string.Format("ProtectionQuotaPercentage: {0}, ", ProtectionQuotaPercentage.HasValue ? ProtectionQuotaPercentage.Value.ToString() : "Null"));
            sb.Append(string.Format("RecordingLifetimePeroid: {0}, ", RecordingLifetimePeriod.HasValue ? RecordingLifetimePeriod.Value.ToString() : "Null"));
            sb.Append(string.Format("CleanupNoticePeroid: {0}, ", CleanupNoticePeroid.HasValue ? CleanupNoticePeroid.Value.ToString() : "Null"));
            sb.Append(string.Format("IsSeriesRecordingEnabled: {0}, ", IsSeriesRecordingEnabled.HasValue ? IsSeriesRecordingEnabled.Value.ToString() : "Null"));
            sb.Append(string.Format("IsRecordingPlaybackNonEntitledChannelEnabled: {0}, ", IsRecordingPlaybackNonEntitledChannelEnabled.HasValue ? IsRecordingPlaybackNonEntitledChannelEnabled.Value.ToString() : "Null"));
            sb.Append(string.Format("IsRecordingPlaybackNonExistingChannelEnabled: {0}, ", IsRecordingPlaybackNonExistingChannelEnabled.HasValue ? IsRecordingPlaybackNonExistingChannelEnabled.Value.ToString() : "Null"));
                        
            return sb.ToString();
        }

    }
}