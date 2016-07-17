using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.QueueObjects
{
    public class FirstFollowerRecordingData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_first_follower_recording";
        
        private long DomainId;
        private string ChannelId;
        private string SeriesId;
        private int SeasonNumber;

        public FirstFollowerRecordingData(int groupId, long domainId, string channelId, string seriesId, int seasonNumber) :
            base(// id = guid
                 Guid.NewGuid().ToString(),
                // task = const
                 TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.DomainId = domainId;
            this.ChannelId = channelId;
            this.SeriesId = seriesId;
            this.SeasonNumber = seasonNumber;

            this.args = new List<object>()
            {
                groupId,
                domainId,
                channelId,
                seriesId,
                seasonNumber
            };
        }
    }
}
