using System;
using System.Collections;
using System.Collections.Generic;

namespace ApiObjects.BulkUpload
{
    public class EpgProgramBulkUploadObject : IBulkUploadObject
    {
        public programme ParsedProgramObject { get; set; }
        public IList<EpgCB> EpgCbObjects { get; set; }
        public int ParentGroupId { get; set; }
        public int GroupId { get; set; }
        public int ChannelId { get; set; }
        public string ChannelExternalId { get; set; }
        public long LinearMediaId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}