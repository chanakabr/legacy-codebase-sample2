using ApiObjects.Catalog;
using System;

namespace ApiObjects.Notification
{
    public class BookmarkEvent : CoreObject
    {
        public long UserId { get; set; }
        public long HouseholdId { get; set; }
        public long AssetId { get; set; }
        public long FileId { get; set; }
        public int Position { get; set; }
        public MediaPlayActions Action { get; set; }
        public eTransactionType ProductType { get; set; }
        public int ProductId { get; set; }

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        protected override bool DoDelete()
        {
            throw new NotImplementedException();
        }

        protected override bool DoInsert()
        {
            throw new NotImplementedException();
        }

        protected override bool DoUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
