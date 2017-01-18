using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.QueueObjects
{
    public class SeriesRecordingTaskData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_series_recording_task";

        private string UserId;
        private long DomainId;
        public string ChannelId { get; set; }
        public string SeriesId { get; set; }
        public int SeasonNumber { get; set; }
        private eSeriesRecordingTask SeriesRecordingTaskType;

        public SeriesRecordingTaskData(int groupId, string userId, long domainId, string channelId, string seriesId, int seasonNumber, eSeriesRecordingTask seriesRecordingTaskType) :
            base(// id = guid
                 Guid.NewGuid().ToString(),
                // task = const
                 TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.UserId = userId;
            this.DomainId = domainId;
            this.ChannelId = channelId;
            this.SeriesId = seriesId;
            this.SeasonNumber = seasonNumber;
            this.SeriesRecordingTaskType = seriesRecordingTaskType;

            this.args = new List<object>()
            {
                groupId,
                userId,
                domainId,
                channelId,
                seriesId,
                seasonNumber,
                seriesRecordingTaskType
            };
        }
    }
}
