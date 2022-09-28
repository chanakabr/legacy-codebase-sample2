using System;
using System.Collections;
using System.Collections.Generic;
using OTT.Lib.MongoDB;

namespace ApiObjects.BulkUpload
{
    [Serializable]
    [MongoDbIgnoreExternalElements]
    // TODO: Move and merge with all other epg objects    
    public class EpgProgramBulkUploadObject : IBulkUploadObject, IAffectedObject, IEquatable<EpgProgramBulkUploadObject>
    {
        public long BulkUploadId { get; set; }
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
        public DateTime UpdateDate { get; set; }
        public bool IsAutoFill { get; set; }
        public IDictionary<string, string> CbDocumentIdsMap { get; set; }
        
        ulong IAffectedObject.ObjectId
        {
            get => EpgId;
        }

        public bool Equals(EpgProgramBulkUploadObject other)
        {
            if (other is null)
                return false;

            return this.EpgId == other.EpgId
                   && this.EpgExternalId == other.EpgExternalId
                   && this.ChannelId == other.ChannelId;
        }


        public bool StartsBefore(EpgProgramBulkUploadObject otherProg) => this.StartDate < otherProg.StartDate;
        public bool StartsBeforeOrWith(EpgProgramBulkUploadObject otherProg) => this.StartDate <= otherProg.StartDate;

        public bool StartsAfter(EpgProgramBulkUploadObject otherProg) => this.StartDate > otherProg.StartDate;
        public bool StartsAfterOrWith(EpgProgramBulkUploadObject otherProg) => this.StartDate >= otherProg.StartDate;

        public bool EndsBefore(EpgProgramBulkUploadObject otherProg) => this.EndDate < otherProg.EndDate;
        public bool EndsBeforeOrWith(EpgProgramBulkUploadObject otherProg) => this.EndDate <= otherProg.EndDate;

        public bool EndsAfter(EpgProgramBulkUploadObject otherProg) => this.EndDate < otherProg.EndDate;
        public bool EndsAfterOrWith(EpgProgramBulkUploadObject otherProg) => this.EndDate <= otherProg.EndDate;

        public bool IsInMiddle(EpgProgramBulkUploadObject otherProg) => StartsAfterOrWith(otherProg) && EndsBeforeOrWith(otherProg) && !IsSameTimeAs(otherProg);

        public bool IsSameTimeAs(EpgProgramBulkUploadObject otherProg) => StartDate == otherProg.StartDate && EndDate == otherProg.EndDate;

        public override bool Equals(object obj) => Equals(obj as EpgProgramBulkUploadObject);

        public override int GetHashCode() => Tuple.Create(EpgId, EpgExternalId, ChannelId).GetHashCode();

        public override string ToString()
        {
            return $"{{EpgId:{EpgId}, EpgExternalId:{EpgExternalId}, ChannelId:{ChannelId}, Time:{PrettyFormatDateRange(StartDate, EndDate)}, UpdateDate:{UpdateDate}}} ";
        }
        
        private string PrettyFormatDateRange(DateTime start, DateTime end)
        {
            var startStr = $"{start:yyyy-MM-dd HH:mm}";
            var endStr = (start.Date != end.Date) ? $"{end:yyyy-MM-dd HH:mm}" : $"{end:HH:mm}";
            return $"{startStr} - {endStr}";
        }
    }
}