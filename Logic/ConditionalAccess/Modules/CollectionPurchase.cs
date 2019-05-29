using ApiObjects;
using Core.ConditionalAccess.Modules;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiLogic.ConditionalAccess.Modules
{
    public class CollectionPurchase : Purchase
    {
        #region Members

        public override eTransactionType type => eTransactionType.Collection;

        public string productId { get; set; }
        public int maxNumberOfUses { get; set; }
        public int viewLifeCycle { get; set; }
        public DateTime collectionStartDate { get; set; }
        public DateTime collectionEndDate { get; set; }
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
                                                                     collectionStartDate, collectionEndDate, createAndUpdateDate, houseHoldId, billingGuid);
            return purchaseId > 0;
        }

        protected override bool DoUpdate()
        {
            throw new NotImplementedException();
        }
    }
}
