using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class SeriesRecording : DomainSeriesRecording
    {
        public ApiObjects.Response.Status Status { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public bool isExternalRecording { get; set; }

        public SeriesRecording()
            : base()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.Id = 0;
        }

        public SeriesRecording(Recording record)
        {
            this.Status = record.Status != null ? new Status(record.Status.Code, record.Status.Message) : null;
            this.Id = record.Id;
            this.EpgId = record.EpgId;
            this.EpgChannelId = record.ChannelId;
            this.Type = record.Type;
        }

        public SeriesRecording(SeriesRecording seriesRecording)
        {
            this.EpgId = seriesRecording.EpgId;
            this.EpgChannelId = seriesRecording.EpgChannelId;
            this.Id = seriesRecording.Id;
            this.SeasonNumber = seriesRecording.SeasonNumber;
            this.SeriesId = seriesRecording.SeriesId;
            this.Status = seriesRecording.Status;
            this.Type = RecordingType.Series;
            this.CreateDate = seriesRecording.CreateDate;
            this.UpdateDate = seriesRecording.UpdateDate;
            this.SeriesRecordingOption = seriesRecording.SeriesRecordingOption ?? new SeriesRecordingOption();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append(string.Format("Id: {0}, ", Id));
            sb.Append(string.Format("EpgID: {0}, ", EpgId));
            sb.Append(string.Format("EpgChannelId: {0}, ", EpgChannelId));
            sb.Append(string.Format("SeriesId: {0}, ", SeriesId));
            sb.Append(string.Format("SeasonNumber: {0}, ", SeasonNumber));
            sb.Append(string.Format("Type: {0}, ", Type.ToString()));
            sb.Append(string.Format("CreateDate: {0}, ", CreateDate != null ? CreateDate.ToString() : ""));
            sb.Append(string.Format("UpdateDate: {0}, ", UpdateDate != null ? UpdateDate.ToString() : ""));
            if (SeriesRecordingOption != null)
            {
                sb.Append(string.Format("SeriesRecordingOption: {0}, ", SeriesRecordingOption.ToString()));
            }
            return sb.ToString();
        }
    }

    public class SeriesRecordingOption
    {
        public int? MinSeasonNumber { get; set; }
        public int? MinEpisodeNumber { get; set; }
        public ChronologicalRecordStartTime ChronologicalRecordStartTime { get; set; }
        
        //Inner prop to store calculated value
        public long? StartDateRecording { get; set; }

        public bool IsValid()
        {
            return MinEpisodeNumber.HasValue && MinSeasonNumber.HasValue || StartDateRecording.HasValue;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (MinEpisodeNumber.HasValue)
                sb.Append($"MinEpisodeNumber: {MinEpisodeNumber}, ");
            if (MinSeasonNumber.HasValue)
                sb.Append($"MinSeasonNumber: {MinSeasonNumber}, ");
            if (StartDateRecording.HasValue)
                sb.Append($"StartDateRecording: {StartDateRecording}, ");

            sb.Append($"ChronologicalRecordStartTime: {ChronologicalRecordStartTime}, ");

            return sb.ToString();
        }
    }

    public enum ChronologicalRecordStartTime
    {
        None = 0,
        Now,
        EpgStartTime
    }
}
