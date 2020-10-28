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

        public CRUDOperations<EpgProgramBulkUploadObject> CrudOperations { get; set; }

        public override string ToString()
        {
            return $"{{{nameof(DateOfProgramsToIngest)}={DateOfProgramsToIngest}, {nameof(CrudOperations)}={CrudOperations}, {nameof(BulkUploadId)}={BulkUploadId}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}