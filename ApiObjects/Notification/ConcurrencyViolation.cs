using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Notification
{
    public class ConcurrencyViolation : CoreObject
    {
        public long Timestamp { get; set; }
        public string UDID { get; set; }
        public string AssetId { get; set; }
        public string ViolationRule { get; set; }
        public string HouseholdId { get; set; }
        public string UserId { get; set; }

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
