using EventBus.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.EventBus
{
    public class CampaignUserEvent : ServiceEvent
    {
        public long Id { get; set; }
        public int Status { get; set; }
    }
}
