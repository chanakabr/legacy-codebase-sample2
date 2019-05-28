using System;
using System.Collections;
using System.Collections.Generic;

namespace ApiObjects.BulkUpload
{
    // TODO: Move and merge with all other epg objects
    public class EpgProgramBulkUploadObject : IBulkUploadObject
    {
        public programme ParsedProgramObject { get; set; }
        public List<EpgCB> EpgCbObjects { get; set; }
        public ulong EpgId { get; set; }
        public string EpgExternalId { get; set; }
        public int ParentGroupId { get; set; }
        public int GroupId { get; set; }
        public int ChannelId { get; set; }
        public string ChannelExternalId { get; set; }
        public long LinearMediaId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public override string ToString()
        {
            return $"{{EpgId:{EpgId}, EpgExternalId:{EpgExternalId}, ChannelId:{ChannelId}, StartDate:{StartDate}, EndDate:{EndDate}}}";
        }
    }
}