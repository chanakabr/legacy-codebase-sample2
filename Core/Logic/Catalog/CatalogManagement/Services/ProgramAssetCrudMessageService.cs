using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiLogic.Api.Managers;
using ApiObjects;
using Confluent.Kafka;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.Generated.Api.Events.Crud.ProgramAsset;
using SchemaRegistryEvents.Catalog;
using TVinciShared;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class ProgramAssetCrudMessageService : IProgramAssetCrudMessageService
    {
        private readonly IAssetManager _assetManager;
        private readonly IEpgAssetManager _epgAssetManager;
        private readonly IKafkaProducer<string, ProgramAsset> _programAssetProducer;
        private readonly ILogger _logger;

        public ProgramAssetCrudMessageService(IAssetManager assetManager, IEpgAssetManager epgAssetManager, IKafkaProducerFactory producerFactory, IKafkaContextProvider contextProvider, ILogger logger)
        {
            _assetManager = assetManager;
            _epgAssetManager = epgAssetManager;
            _programAssetProducer = producerFactory.Get<string, ProgramAsset>(contextProvider, Partitioner.Murmur2Random);
            _logger = logger;
        }

        public Task PublishCreateEventAsync(long groupId, long epgId)
        {
            return PublishKafkaCreateOrUpdateEventsAsync(groupId, CrudOperationType.CREATE_OPERATION, new[] { epgId });
        }

        public Task PublishCreateEventsAsync(long groupId, IReadOnlyCollection<long> epgIds)
        {
            return PublishKafkaCreateOrUpdateEventsAsync(groupId, CrudOperationType.CREATE_OPERATION, epgIds);
        }

        public Task PublishUpdateEventAsync(long groupId, long epgId)
        {
            return PublishKafkaCreateOrUpdateEventsAsync(groupId, CrudOperationType.UPDATE_OPERATION, new[] { epgId });
        }

        public Task PublishUpdateEventsAsync(long groupId, IReadOnlyCollection<long> epgIds)
        {
            return PublishKafkaCreateOrUpdateEventsAsync(groupId, CrudOperationType.UPDATE_OPERATION, epgIds);
        }

        public Task PublishDeleteEventAsync(long groupId, long epgId)
        {
            return PublishDeleteEventsAsync(groupId, new[] { epgId });
        }

        public Task PublishDeleteEventsAsync(long groupId, IReadOnlyCollection<long> epgIds)
        {
            if (_programAssetProducer == null)
            {
                _logger.LogError($"{nameof(ProgramAsset)} message with parameters {nameof(groupId)}={groupId}, {nameof(epgIds)}=[{string.Join(",", epgIds)}], operation={CrudOperationType.DELETE_OPERATION} can not be published : {nameof(_programAssetProducer)} is null.");
                return Task.CompletedTask;
            }

            var publishTasks = new List<Task>();
            foreach (var epgId in epgIds)
            {
                var programAssetEvent = new ProgramAsset
                {
                    Id = epgId,
                    PartnerId = groupId,
                    Operation = CrudOperationType.DELETE_OPERATION
                };

                var task = _programAssetProducer.ProduceAsync(ProgramAsset.GetTopic(), programAssetEvent.GetPartitioningKey(), programAssetEvent);
                publishTasks.Add(task);
            }

            return Task.WhenAll(publishTasks);
        }

        private Task PublishKafkaCreateOrUpdateEventsAsync(long groupId, long operation, IReadOnlyCollection<long> epgIds)
        {
            var epgAssets = _epgAssetManager.GetEpgAssets(groupId, epgIds).ToArray();

            var assets = epgAssets
                .Where(x => x.LinearAssetId.HasValue)
                .Select(x => x.LinearAssetId.Value)
                .Distinct()
                .Select(x => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, x))
                .ToList();
            var liveAssets = _assetManager
                .GetAssets((int)groupId, assets, true)
                ?.OfType<LiveAsset>()
                .ToArray();

            var publishTasks = new List<Task>();
            foreach (var epgId in epgIds)
            {
                var epgAsset = epgAssets.FirstOrDefault(x => x.Id == epgId);
                if (epgAsset == null)
                {
                    _logger.LogError($"{nameof(ProgramAsset)} message with parameters {nameof(groupId)}={groupId}, {nameof(ProgramAsset.Id)}={epgId}, operation={operation} can not be published : {nameof(epgAsset)} is null.");
                    continue;
                }

                var liveAsset = liveAssets?.FirstOrDefault(x => x.Id == epgAsset.LinearAssetId);

                var task = PublishKafkaCreateOrUpdateEventAsync(groupId, operation, liveAsset, epgAsset);
                publishTasks.Add(task);
            }

            return Task.WhenAll(publishTasks);
        }

        private Task PublishKafkaCreateOrUpdateEventAsync(long groupId, long operation, LiveAsset liveAsset, EpgAsset epgAsset)
        {
            if (_programAssetProducer == null)
            {
                _logger.LogError($"{nameof(ProgramAsset)} message with parameters {nameof(groupId)}={groupId}, {nameof(ProgramAsset.Id)}={epgAsset.Id}, operation={operation} can not be published : {nameof(_programAssetProducer)} is null.");
                return Task.CompletedTask;
            }

            if (liveAsset == null)
            {
                _logger.LogError($"{nameof(ProgramAsset)} message with parameters {nameof(groupId)}={groupId}, {nameof(ProgramAsset.Id)}={epgAsset.Id}, operation={operation} can not be published : {nameof(liveAsset)} is null.");
                return Task.CompletedTask;
            }

            var expirationDate = epgAsset.CatchUpEnabled == true
                ? epgAsset.EndDate?.AddMinutes(liveAsset.SummedCatchUpBuffer)
                : epgAsset.EndDate;
            var programAssetEvent = new ProgramAsset
            {
                Id = epgAsset.Id,
                EndDate = epgAsset.EndDate?.ToUtcUnixTimestampSeconds(),
                ExpirationDate = expirationDate?.ToUtcUnixTimestampSeconds(),
                ExternalOfferIds = (epgAsset.ExternalOfferIds ?? new List<string>(0)).ToArray(),
                PartnerId = groupId,
                StartDate = epgAsset.StartDate?.ToUtcUnixTimestampSeconds(),
                Operation = operation
            };

            return _programAssetProducer.ProduceAsync(ProgramAsset.GetTopic(), programAssetEvent.GetPartitioningKey(), programAssetEvent);
        }
    }
}