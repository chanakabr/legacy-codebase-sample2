using System;
using System.Collections.Generic;
using EventBus.Abstraction;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class LineupNotificationRequestedEvent : ServiceEvent
    {
        public List<long> RegionIds { get; set; }
    }
}