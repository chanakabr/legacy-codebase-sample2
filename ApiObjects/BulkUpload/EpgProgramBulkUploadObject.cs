using System;
using System.Collections;
using System.Collections.Generic;

namespace ApiObjects.BulkUpload
{
    // TODO: Move and merge with all other epg objects
    public class EpgProgramBulkUploadObject : IBulkUploadObject, IAffectedObject, IEquatable<EpgProgramBulkUploadObject>
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
        public bool IsAutoFill { get; set; }
        ulong IAffectedObject.ObjectId { get => EpgId; }

        public bool Equals(EpgProgramBulkUploadObject other)
        {
            if (other is null)
                return false;

            return this.EpgId == other.EpgId 
                && this.EpgExternalId == other.EpgExternalId
                && this.ChannelId == other.ChannelId;
        }

        public override bool Equals(object obj) => Equals(obj as EpgProgramBulkUploadObject);

        public override int GetHashCode() => Tuple.Create(EpgId, EpgExternalId, ChannelId).GetHashCode();

        public override string ToString()
        {
            return $"{{EpgId:{EpgId}, EpgExternalId:{EpgExternalId}, ChannelId:{ChannelId}, StartDate:{StartDate}, EndDate:{EndDate}}}";
        }
    }
}