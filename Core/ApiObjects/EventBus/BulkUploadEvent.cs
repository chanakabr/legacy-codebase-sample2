using EventBus.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.EventBus
{
    [Serializable]
    public abstract class BulkUploadEvent : ServiceEvent
    {
        public long BulkUploadId { get; set; }

        public BulkUploadEvent()
        {
            // Bulk Upload events are always targeted 
            base.EventForGroupDedicatedConsumer = true;
        }
    }
}
