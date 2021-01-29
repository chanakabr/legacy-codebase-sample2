using Newtonsoft.Json;
using System;

namespace ApiObjects
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class UnifiedChannel
    {
        public long Id { get; set; }

        public UnifiedChannelType Type { get; set; }
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class UnifiedChannelInfo : UnifiedChannel
    {
        public string Name { get; set; }
        
        public TimeSlot TimeSlot { get; set; }

        public UnifiedChannelInfo()
        {
            TimeSlot = new TimeSlot();
        }
    }

    public class UnifiedChannelInfoDTO : UnifiedChannel
    {
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public enum UnifiedChannelType
    {
        Internal,
        External
    }
}