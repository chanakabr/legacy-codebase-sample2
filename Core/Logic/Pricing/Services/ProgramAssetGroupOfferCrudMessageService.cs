using System.Threading.Tasks;
using Confluent.Kafka;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Crud.ProgramAssetGroupOffer;
using SchemaRegistryEvents.Catalog;
using TVinciShared;

namespace Core.Pricing.Services
{
    public class ProgramAssetGroupOfferCrudMessageService : IProgramAssetGroupOfferCrudMessageService
    {
        private readonly IKafkaProducer<string, ProgramAssetGroupOffer> _pagoProducer;
        
        public ProgramAssetGroupOfferCrudMessageService(IKafkaProducerFactory producerFactory, IKafkaContextProvider contextProvider)
        {
            _pagoProducer = producerFactory.Get<string, ProgramAssetGroupOffer>(contextProvider, Partitioner.Murmur2Random);
        }

        public Task PublishCreateEventAsync(int groupId, ApiObjects.Pricing.ProgramAssetGroupOffer programAssetGroupOffer)
        {
            return PublishCreateOrUpdate(CrudOperationType.CREATE_OPERATION, groupId, programAssetGroupOffer);
        }

        public Task PublishUpdateEventAsync(int groupId, ApiObjects.Pricing.ProgramAssetGroupOffer programAssetGroupOffer)
        {
            return PublishCreateOrUpdate(CrudOperationType.UPDATE_OPERATION, groupId, programAssetGroupOffer);
        }

        public Task PublishDeleteEventAsync(int groupId, long pagoId)
        {
            var e = new ProgramAssetGroupOffer()
            {
                PartnerId = groupId,
                Id = pagoId,
                Operation = CrudOperationType.DELETE_OPERATION
            };

            return _pagoProducer.ProduceAsync(ProgramAssetGroupOffer.GetTopic(), e.GetPartitioningKey(), e);
        }

        private Task PublishCreateOrUpdate(long operation, int groupId,
            ApiObjects.Pricing.ProgramAssetGroupOffer programAssetGroupOffer)
        {
            var e = new ProgramAssetGroupOffer()
                {
                    PartnerId = groupId,
                    ExpiryDate = programAssetGroupOffer.ExpiryDate?.ToUtcUnixTimestampSeconds(),
                    ExternalOfferId = programAssetGroupOffer.ExternalOfferId,
                    StartDate = programAssetGroupOffer.StartDate?.ToUtcUnixTimestampSeconds(),
                    Id = programAssetGroupOffer.Id,
                    Operation = operation
                };

            return _pagoProducer.ProduceAsync(ProgramAssetGroupOffer.GetTopic(), e.GetPartitioningKey(), e);
        }
    }
}