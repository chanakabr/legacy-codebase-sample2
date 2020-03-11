using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Abstraction
{
    public abstract class DelayedServiceEvent : ServiceEvent
    {
        public DateTime ETA { get; set; }
    }
}
