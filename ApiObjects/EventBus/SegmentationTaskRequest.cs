using EventBus.Abstraction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class SegmentationTaskRequest : ServiceEvent
    {
        [JsonProperty("task_type")]
        public SegmentationTaskType TaskType { get; set; }

        [JsonProperty("users_segments")]
        public Dictionary<string, List<long>> UsersSegments { get; set; }

        [JsonProperty("segment_affected_users")]
        public Dictionary<long, int> SegmentAffectedUsers { get; set; }

        [JsonProperty("process_id")]
        public string ProcessId { get; set; }

        [JsonProperty("message_count")]
        public int MessageCount { get; set; }

        [JsonProperty("total_message_count")]
        public int TotalMessageCount { get; set; }

        public override string ToString()
        {
            return $"{{{nameof(TaskType)}={TaskType}, {nameof(UsersSegments)}={UsersSegments}, {nameof(SegmentAffectedUsers)}={SegmentAffectedUsers}, {nameof(ProcessId)}={ProcessId}, {nameof(MessageCount)}={MessageCount}, {nameof(TotalMessageCount)}={TotalMessageCount}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }

    public enum SegmentationTaskType
    {
        update_user_segments,
        update_segment_affected_users
    }
}