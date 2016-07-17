using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class SeriesRecording
    {
        public ApiObjects.Response.Status Status { get; set; }

        public long Id { get; set; }

        public long EpgId { get; set; }

        public long ChannelId { get; set; }

        public string SeriesId { get; set; } 
        
        public int SeasonNumber { get; set; }

        public RecordingType Type { get; set; }

        public SeriesRecording()
        {
        }

        public SeriesRecording(SeriesRecording seriesRecording)
        {
            this.EpgId = seriesRecording.EpgId;
            this.ChannelId = seriesRecording.ChannelId;
            this.Id = seriesRecording.Id;
            this.SeasonNumber = seriesRecording.SeasonNumber;
            this.SeriesId = seriesRecording.SeriesId;
            this.Status = seriesRecording.Status;
            this.Type = RecordingType.Series;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append(string.Format("Id: {0}, ", Id));
            sb.Append(string.Format("EpgID: {0}, ", EpgId));
            sb.Append(string.Format("ChannelId: {0}, ", ChannelId)); 
            sb.Append(string.Format("SeriesId: {0}, ", SeriesId));           
            sb.Append(string.Format("SeasonNumber: {0}, ", SeasonNumber));
            sb.Append(string.Format("Type: {0}, ", Type.ToString()));
                     
            return sb.ToString();
        }
    }
}
