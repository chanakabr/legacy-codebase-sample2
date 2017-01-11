using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConditionalAccess.Modules
{
    public abstract class Entitlement : CoreObject
    {
        #region members
        public EntitlementAction action {get; set;}

        public string siteGuid { get; set; }
        public string currency { get; set; }
        public string customData { get; set; }
        public string country { get; set; }
        public string deviceName { get; set; }
        public string billingGuid { get; set; }
        
        public double price { get; set; }

        public long billingTransactionId { get; set; }
        public long houseHoldId { get; set; }
        public long purchaseId { get; set; }

        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }

        #endregion

        public Entitlement(int groupId, EntitlementAction action)
        {
            this.GroupId = groupId;
            this.action = action;
        }

        public abstract override CoreObject CoreClone();       

        protected override bool DoInsert()
        {
             throw new NotImplementedException();
        }

        protected override bool DoUpdate()
        {
             throw new NotImplementedException();
        }

        protected override bool DoDelete()
        {
             throw new NotImplementedException();
        }
    }
}
