using ApiObjects;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConditionalAccess.Modules
{
    public class SubscriptionEntitlement : Entitlement
    {
        #region member

        public int productId { get; set; }
        public int maxNumberOfViews { get; set; }
        public int viewLifeCycle { get; set; }
         
        public bool isEntitledToPreviewModule { get; set; }
        public bool usageModuleExists { get; set; }
        public bool isRecurring { get; set; }

        public long previewModuleId { get; set; }

        public DateTime? entitlementDate { get; set; }

        #endregion

        public SubscriptionEntitlement(int groupId, EntitlementAction action)
            : base(groupId, action) 
        {
        }

        public SubscriptionEntitlement Clone()
        {
            return CloneImpl();
        }

        protected virtual SubscriptionEntitlement CloneImpl()
        {
            var copy = (SubscriptionEntitlement)MemberwiseClone();

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
                switch (action)
                {
                    case EntitlementAction.Purchase:
                        this.purchaseId = ConditionalAccessDAL.Insert_NewMPPPurchase(this.GroupId, this.productId.ToString(), this.siteGuid, this.isEntitledToPreviewModule ? 0.0 : this.price, this.currency, this.customData, this.country,
                               this.deviceName, this.usageModuleExists ? this.maxNumberOfViews : 0, this.usageModuleExists ? this.viewLifeCycle : 0, this.isRecurring, this.billingTransactionId,
                               this.previewModuleId, this.startDate.Value, this.endDate.Value, this.entitlementDate.Value, this.houseHoldId, this.billingGuid);
                        if (this.purchaseId > 0)
                        {
                            success = true;
                        }
                        break;
                    default:
                        break;
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
