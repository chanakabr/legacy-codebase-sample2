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

        public override string ToString()
        {
            return $"VirtualAssetInfo: Id: {Id}, Type: {Type}, UserId: {UserId}";
        }
    }
}