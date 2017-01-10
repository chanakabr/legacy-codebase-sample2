using ApiObjects;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConditionalAccess.Modules
{
    public class PpvPurchase : CoreObject
    {
        #region member
        public int contentId { get; set; }
        public int maxNumOfViews { get; set; }

        public double price { get; set; }
        public long billingTransactionId { get; set; }
        public long houseHoldId { get; set; }
        public long purchaseId { get; set; }

        public string siteGuid { get; set; }
        public string currency { get; set; }
        public string customData { get; set; }
        public string subscriptionCode { get; set; }
        public string country { get; set; }
        public string deviceName { get; set; }
        public string billingGuid { get; set; }

        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public DateTime entitlementDate { get; set; }
        #endregion

        public PpvPurchase(int groupId)
        {
            this.GroupId = groupId;
        }

        public PpvPurchase Clone()
        {
            return CloneImpl();
        }

        protected virtual PpvPurchase CloneImpl()
        {
            var copy = (PpvPurchase)MemberwiseClone();

            return copy;
        }

        public override CoreObject CoreClone()
        {
            return this.Clone();
        }

        protected override bool DoInsert()
        {
            bool success = false;

            try
            {
                this.purchaseId = ConditionalAccessDAL.Insert_NewPPVPurchase(this.GroupId, this.contentId, this.siteGuid, this.price, this.currency, this.maxNumOfViews,
                                                                        this.customData, this.subscriptionCode, this.billingTransactionId, this.startDate, this.endDate,
                                                                        this.entitlementDate, this.country, string.Empty, this.deviceName, this.houseHoldId, this.billingGuid);
                this.purchaseId = 0;
                if (this.purchaseId > 0)
                {
                    success = true;
                }
            }
            catch
            {
            }
            return success;
        }

        protected override bool DoUpdate()
        {
            bool success = false;

            return success;
        }

        protected override bool DoDelete()
        {
            throw new NotImplementedException();
        }
    }
}
