using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConditionalAccess.Modules
{
    public abstract class Purchase : CoreObject
    {
        #region members

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

        public Purchase(int groupId)
        {
            this.GroupId = groupId;
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
