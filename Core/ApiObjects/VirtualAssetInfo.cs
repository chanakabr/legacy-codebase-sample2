using ApiObjects.Response;
using System;

namespace ApiObjects
{
    public class VirtualAssetInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public ObjectVirtualAssetInfoType Type { get; set; }

        public long UserId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool withExtendedTypes { get; set; }

        public long? DuplicateAssetId { get; set; }

        public bool? IsActive { get; set; }

        public long? AssetUserRuleId { get; set; }

        public override string ToString()
        {
            return $"VirtualAssetInfo: Id: {Id}, Type: {Type}, UserId: {UserId}";
        }
    }

    public class VirtualAssetInfoResponse
    {
        public VirtualAssetInfoStatus Status { get; set; }
        public long AssetId { get; set; }
        public Status ResponseStatus { get; set; }
    }

    public enum VirtualAssetInfoStatus
    {
        NotRelevant = 0,
        OK = 1,
        Error = 2,
    }

}