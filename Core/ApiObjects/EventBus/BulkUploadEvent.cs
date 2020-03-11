using EventBus.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.EventBus
{
    public abstract class BulkUploadEvent : ServiceEvent
    {
        public long BulkUploadId { get; set; }
    }
}
