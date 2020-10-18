using ApiObjects;
using Core.ConditionalAccess.Modules;
using DAL;
using System;

namespace ApiLogic.ConditionalAccess.Modules
{
    public class CollectionPurchase : Purchase
    {
        #region Members

        public override eTransactionType type => eTransactionType.Collection;
        public string productId { get; set; }
        public int maxNumberOfUses { get; set; }
        public int viewLifeCycle { get; set; }
        public DateTime createAndUpdateDate { get; set; }

        #endregion

        public CollectionPurchase(int groupId)
            : base(groupId)
        {
        }
        
        public override CoreObject CoreClone()
        {
            return (CollectionPurchase)MemberwiseClone();
        }

        protected override bool DoDelete()
        {
            return ConditionalAccessDAL.CancelCollectionPurchaseTransaction(siteGuid, int.Parse(productId), (int)houseHoldId);
        }

        protected override bool DoInsert()
        {
            purchaseId = ConditionalAccessDAL.Insert_NewMColPurchase(GroupId, productId, siteGuid, price, currency, customData, country,
                                                                     deviceName, maxNumberOfUses,
                                                                     viewLifeCycle, billingTransactionId,
                                                                     startDate.Value, endDate.Value, createAndUpdateDate, houseHoldId, billingGuid, this.IsPending);
            return purchaseId > 0;
        }

        protected override bool DoUpdate()
        {            
            bool success = ConditionalAccessDAL.UpdateCollectionPurchases(this.GroupId, this.purchaseId, this.startDate.Value, this.endDate.Value, this.IsPending);
            return success;
        }
    }
}
