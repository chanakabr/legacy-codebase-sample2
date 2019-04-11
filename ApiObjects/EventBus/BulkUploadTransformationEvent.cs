using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Abstraction;

namespace ApiObjects.EventBus
{
    public class BulkUploadTransformationEvent : ServiceEvent
    {
        public long BulkUploadId { get; set; }
    }

}
