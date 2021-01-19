using ApiObjects.Catalog;
using System;
using ApiObjects;

namespace ApiObjects.Notification
{
    public class AssetEvent : CoreObject
    {
        public long UserId { get; set; }        
        public long AssetId { get; set; }
        public int Type { get; set; }
        public string ExternalId { get; set; }

        public override CoreObject CoreClone()
        {
            return this;
        }

        protected override bool DoDelete()
        {
            return true;
        }

        protected override bool DoInsert()
        {
            return true;
        }

        protected override bool DoUpdate()
        {
            return true;
        }
    }

    public class EpgAssetEvent : AssetEvent
    {
        public long LiveAssetId { get; set; }
    }
}