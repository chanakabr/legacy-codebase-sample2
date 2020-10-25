using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiObjects.BulkUpload;
using EventBus.Abstraction;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class BulkUploadIngestEvent : BulkUploadEvent
    {        
        public DateTime DateOfProgramsToIngest { get; set; }

        public List<EpgProgramBulkUploadObject> ProgramsToIngest { get; set; }

        public override string ToString()
        {
            return $"{{{nameof(DateOfProgramsToIngest)}={DateOfProgramsToIngest}, {nameof(ProgramsToIngest)}={string.Join(",", ProgramsToIngest)}, {nameof(BulkUploadId)}={BulkUploadId}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}