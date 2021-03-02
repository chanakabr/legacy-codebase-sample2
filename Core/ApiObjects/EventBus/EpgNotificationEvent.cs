using EventBus.Abstraction;
using System;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class EpgNotificationEvent : ServiceEvent
    {
        public long LiveAssetId { get; set; }
        public long EpgChannelId { get; set; }
        public Range<DateTime> UpdatedRange { get; set; }
        public bool DisableEpgNotification { get; set; }
    }

    [Serializable]
    public class Range<T>
    {
        public T From { get; }
        public T To { get; }

        public Range(T from, T to)
        {
            From = from;
            To = to;
        }
    }

    public enum EventType
    {
        epg_update = 1
    }
}
