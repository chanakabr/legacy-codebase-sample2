using ApiObjects.BulkUpload;
using EventBus.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.EventBus
{
    public class BulkUploadIngestValidationEvent : BulkUploadEvent
    {
        public List<EpgProgramBulkUploadObject> EPGs { get; set; }
        public DateTime DateOfProgramsToIngest { get; set; }
        public IDictionary<string, LanguageObj> Languages { get; set; }
        public Dictionary<int, Dictionary<string, BulkUploadProgramAssetResult>> Results { get; set; }
        public override string ToString()
        {
            return $"{{{nameof(EPGs)}={EPGs}, {nameof(DateOfProgramsToIngest)}={DateOfProgramsToIngest}, {nameof(Languages)}={Languages}, {nameof(Results)}={Results}, {nameof(BulkUploadId)}={BulkUploadId}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}
