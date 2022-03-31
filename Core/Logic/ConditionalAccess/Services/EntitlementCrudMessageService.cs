using System.ComponentModel;
using System.Threading.Tasks;
using ApiObjects;
using OTT.Lib.Kafka;
using Confluent.Kafka;
using Core.ConditionalAccess.Modules;
using Microsoft.Extensions.Logging;
using Phoenix.Generated.Api.Events.Crud.Entitlement;
using SchemaRegistryEvents.Catalog;
using TVinciShared;

namespace ApiLogic.ConditionalAccess.Services
{
    public class EntitlementCrudMessageService : IEntitlementCrudMessageService
    {
        private readonly IKafkaProducer<string, Entitlement> _entitlementProducer;
        private readonly ILogger _logger;

        public EntitlementCrudMessageService(IKafkaProducerFactory producerFactory, IKafkaContextProvider contextProvider, ILogger logger)
        {
            _entitlementProducer = producerFactory.Get<string, Entitlement>(contextProvider, Partitioner.Murmur2Random);
            _logger = logger;
        }
        
        public Task PublishCreateEventAsync(ProgramAssetGroupOfferPurchase pagoPurchase)
        {
            var e = new Entitlement
            {
                PartnerId = pagoPurchase.GroupId,
                HouseholdId = pagoPurchase.houseHoldId,
                ProductId = pagoPurchase.ProductId,
                EndDate = pagoPurchase.endDate?.ToUtcUnixTimestampSeconds(),
                ProductTypeId = (int)pagoPurchase.type,
                Operation = CrudOperationType.CREATE_OPERATION
            };
            
            return _entitlementProducer.ProduceAsync(Entitlement.GetTopic(), e.GetPartitioningKey(), e);
        }

        public Task PublishUpdateEventAsync(ProgramAssetGroupOfferPurchase pagoPurchase)
        {
            var e = new Entitlement
            {
                PartnerId = pagoPurchase.GroupId,
                HouseholdId = pagoPurchase.houseHoldId,
                ProductId = pagoPurchase.ProductId,
                EndDate = pagoPurchase.endDate?.ToUtcUnixTimestampSeconds(),
                ProductTypeId = (int)pagoPurchase.type,
                Operation = CrudOperationType.UPDATE_OPERATION
            };
            
            return _entitlementProducer.ProduceAsync(Entitlement.GetTopic(), e.GetPartitioningKey(), e);
        }

        
        public Task PublishDeleteEventAsync(ProgramAssetGroupOfferPurchase pagoPurchase)
        {
            var e = new Entitlement
            {
                PartnerId = pagoPurchase.GroupId,
                HouseholdId = pagoPurchase.houseHoldId,
                ProductId = pagoPurchase.ProductId,
                ProductTypeId = (int)pagoPurchase.type,
                Operation = CrudOperationType.DELETE_OPERATION
            };
            
            return _entitlementProducer.ProduceAsync(Entitlement.GetTopic(), e.GetPartitioningKey(), e);
        }
    }
}