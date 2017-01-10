using ApiObjects;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConditionalAccess.Modules
{
    public class SubscriptionPurchase : CoreObject
    {
        #region member

        public int productId { get; set; }
        public int maxNumberOfViews { get; set; }
        public int viewLifeCycle { get; set; }

        public string siteguid { get; set; }
        public string currency { get; set; }
        public string customData { get; set; }
        public string country { get; set; }
        public string deviceName { get; set; }
        public string billingGuid { get; set; }

        public bool isEntitledToPreviewModule { get; set; }
        public bool usageModuleExists { get; set; }
        public bool isRecurring { get; set; }

        public double price { get; set; }
        public long billingTransactionId { get; set; }
        public long previewModuleId { get; set; }
        public long houseHoldId { get; set; }
        public long purchaseId { get; set; }

        public DateTime transactionStartDate { get; set; }
        public DateTime? subscriptionEndDate { get; set; }
        public DateTime? entitlementDate { get; set; }

        #endregion

        public SubscriptionPurchase(int groupId)
        {
            this.GroupId = groupId;
        }

        public SubscriptionPurchase Clone()
        {
            return CloneImpl();
        }

        protected virtual SubscriptionPurchase CloneImpl()
        {
            var copy = (SubscriptionPurchase)MemberwiseClone();

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
                this.purchaseId = ConditionalAccessDAL.Insert_NewMPPPurchase(this.GroupId, this.productId.ToString(), this.siteguid, this.isEntitledToPreviewModule ? 0.0 : this.price, this.currency, this.customData, this.country,
                                this.deviceName, this.usageModuleExists ? this.maxNumberOfViews : 0, this.usageModuleExists ? this.viewLifeCycle : 0, this.isRecurring, this.billingTransactionId,
                                this.previewModuleId, this.transactionStartDate, this.subscriptionEndDate.Value, this.entitlementDate.Value, this.houseHoldId, this.billingGuid);
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
