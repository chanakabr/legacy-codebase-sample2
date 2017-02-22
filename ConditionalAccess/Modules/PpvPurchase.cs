using ApiObjects;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConditionalAccess.Modules
{
    public class PpvPurchase : Purchase
    {
        #region member

        public int contentId { get; set; }
        public int maxNumOfViews { get; set; }
       
        public string subscriptionCode { get; set; }

        public DateTime entitlementDate { get; set; }

        #endregion

        public PpvPurchase(int groupId)
            : base(groupId)
        {            
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
                                                                this.customData, this.subscriptionCode, this.billingTransactionId, this.startDate.Value, this.endDate.Value,
                                                                this.entitlementDate, this.country, string.Empty, this.deviceName, this.houseHoldId, this.billingGuid);
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
