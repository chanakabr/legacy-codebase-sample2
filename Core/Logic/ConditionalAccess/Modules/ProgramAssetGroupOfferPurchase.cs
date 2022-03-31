using ApiObjects;
using DAL;
using System;
using ApiLogic.ConditionalAccess.Services;
using Phx.Lib.Log;
using ApiLogic.Catalog.CatalogManagement.Services;
using EventBus.Kafka;
using OTT.Lib.Kafka;

namespace Core.ConditionalAccess.Modules
{
    public class ProgramAssetGroupOfferPurchase : Purchase
    {
        private static IEntitlementCrudMessageService  _messageService;
        
        public long ProductId { get; set; }

        public DateTime CreateAndUpdateDate { get; set; }

        public override eTransactionType type { get { return eTransactionType.ProgramAssetGroupOffer; } }

        public ProgramAssetGroupOfferPurchase(int groupId) : base(groupId) { }

        public ProgramAssetGroupOfferPurchase Clone() { return CloneImpl(); }

        protected virtual ProgramAssetGroupOfferPurchase CloneImpl()
        {
            return (ProgramAssetGroupOfferPurchase)MemberwiseClone();
        }

        public override CoreObject CoreClone()
        {
            return this.Clone();
        }

        protected override bool DoInsert()
        {
            purchaseId = ConditionalAccessDAL.InsertPagoPurchase(GroupId, ProductId, siteGuid, price, currency, customData, country, deviceName, billingTransactionId, startDate.Value, endDate.Value,
                                            houseHoldId, billingGuid, IsPending, CreateAndUpdateDate);
            var isSuccess = purchaseId > 0;
            if (isSuccess)
                _messageService?.PublishCreateEventAsync(this).GetAwaiter().GetResult();
            
            return isSuccess;
        }

        protected override bool DoUpdate()
        {
            var isSuccess = ConditionalAccessDAL.UpdatePagoPurchase(GroupId, houseHoldId, purchaseId, startDate.Value, endDate.Value, IsPending);
            if(isSuccess)
                _messageService?.PublishUpdateEventAsync(this).GetAwaiter().GetResult();
            return isSuccess;
        }

        protected override bool DoDelete()
        {
            long.TryParse(siteGuid, out long userId);
            var isSuccess = ConditionalAccessDAL.CancelPagoPurchaseTransactionByUser(userId, ProductId);
            if(!isSuccess)
            {
                isSuccess = ConditionalAccessDAL.CancelPagoPurchaseTransactionByHousehold(houseHoldId, ProductId);
            }

            if (isSuccess)
                _messageService?.PublishDeleteEventAsync(this).GetAwaiter().GetResult();
            return isSuccess;
        }
        
        public static void InitEntitlementCrudMessageService(IKafkaContextProvider contextProvider)
        {
            _messageService = new EntitlementCrudMessageService(
                KafkaProducerFactoryInstance.Get(),
                contextProvider,
                new KLogger(nameof(ProgramAssetCrudMessageService)));
        }
    }
}