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
        public List<EpgProgramBulkUploadObject> EdgeProgramsToUpdate { get; set; }
        public DateTime DateOfProgramsToIngest { get; set; }
        public IDictionary<string, LanguageObj> Languages { get; set; }
        public Dictionary<string, BulkUploadProgramAssetResult> Results { get; set; }
    }
}
