using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.BulkUpload;
using EventBus.Abstraction;

namespace ApiObjects.EventBus
{
    public class BulkUploadIngestEvent : BulkUploadEvent
    {
        public DateTime DateOfProgramsToIngest { get; set; }

        public List<EpgProgramBulkUploadObject> ProgramsToIngest { get; set; }
    }
}