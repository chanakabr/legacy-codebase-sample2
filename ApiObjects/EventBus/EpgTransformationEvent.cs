using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventBus.Abstraction;

namespace ApiObjects.EventBus
{
    // TODO ARTHUR - CHANGE THE NAME OF EPG_TRANSFORMATION_EVENT TO TRANSFORMATION_EVENT
    public class EpgTransformationEvent : ServiceEvent
    {
        public int GroupId { get; set; }
        public long UserId { get; set; }
        public BulkUpload.BulkUpload BulkUploadData { get; set; }
    }

}
