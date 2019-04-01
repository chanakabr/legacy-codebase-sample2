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
        public int GroupId { get; set; }
        public long UserId { get; set; }
        public BulkUpload.BulkUpload BulkUploadData { get; set; }
    }

}
