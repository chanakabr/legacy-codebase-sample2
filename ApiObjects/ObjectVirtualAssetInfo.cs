using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects
{
    public class ObjectVirtualAssetInfo
    {
        public int AssetStructId { get; set; }

        public int MetaId { get; set; }

        public ObjectVirtualAssetInfoType Type { get; set; }
    }   

    public enum ObjectVirtualAssetInfoType { Subscription = 0, Segment = 1, Category = 2 }

    public enum ObjectVirtualAssetFilterStatus { None = 0, Results = 1, Error = 2 }

    public class ObjectVirtualAssetFilter
    {
        public List<long> ObjectIds;
        public ObjectVirtualAssetFilterStatus ResultStatus;
        public Status Status;
        public int TotalItems;

    }
}