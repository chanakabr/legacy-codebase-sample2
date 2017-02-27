using ApiObjects;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.ConditionalAccess.Modules
{
    public class SubscriptionPurchase : Purchase
    {
        #region member

        public string productId { get; set; }

        public int maxNumberOfViews { get; set; }
        public int viewLifeCycle { get; set; }
         
        public bool isEntitledToPreviewModule { get; set; }
        public bool usageModuleExists { get; set; }
        public bool isRecurring { get; set; }

        public long previewModuleId { get; set; }

        public DateTime? entitlementDate { get; set; }

        public SubscriptionPurchaseStatus status { get; set; }

        #endregion

        public SubscriptionPurchase(int groupId)
            : base(groupId) 
        {
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
                this.purchaseId = ConditionalAccessDAL.Insert_NewMPPPurchase(this.GroupId, this.productId, this.siteGuid, this.isEntitledToPreviewModule ? 0.0 : this.price, this.currency, this.customData, this.country,
                       this.deviceName, this.usageModuleExists ? this.maxNumberOfViews : 0, this.usageModuleExists ? this.viewLifeCycle : 0, this.isRecurring, this.billingTransactionId,
                       this.previewModuleId, this.startDate.Value, this.endDate.Value, this.entitlementDate.Value, this.houseHoldId, this.billingGuid);
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
            try
            {
                long result = ConditionalAccessDAL.CancelSubscription((int)this.purchaseId, this.GroupId, this.siteGuid, this.productId, (int)this.status);
                if (result > 0)
                {
                    success = true;
                }
            }
            catch
            { 
            }
            return success;
        }

        protected override bool DoDelete()
        {
            throw new NotImplementedException();
        }

    }
}
