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
        /// <summary>
        /// from version 7.1.1 this property is no longer sent.
        /// Ingest handler will have backwad compatability code applied to facilitate the transition from lower versions.
        /// If you see this code and wonder why its here and all costumers already on versions higher than 7.1.1 its safe to remove
        /// </summary>
        public DateTime DateOfProgramsToIngest { get; set; }
        
        public string TargetIndexName { get; set; }

        public CRUDOperations<EpgProgramBulkUploadObject> CrudOperations { get; set; }

        public override string ToString()
        {
            return $"{{{nameof(TargetIndexName)}={TargetIndexName}, {nameof(CrudOperations)}={CrudOperations}, {nameof(BulkUploadId)}={BulkUploadId}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}