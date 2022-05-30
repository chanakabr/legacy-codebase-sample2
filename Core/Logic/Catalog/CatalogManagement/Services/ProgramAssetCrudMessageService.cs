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

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class ProgramAssetCrudMessageService : IProgramAssetCrudMessageService
    {
        private readonly IAssetManager _assetManager;
        private readonly IEpgAssetManager _epgAssetManager;
        private readonly IProgramCrudEventMapper _programCrudEventMapper;
        private readonly IKafkaProducer<string, ProgramAsset> _programAssetProducer;
        private readonly ILogger _logger;

        public ProgramAssetCrudMessageService(
            IAssetManager assetManager,
            IEpgAssetManager epgAssetManager,
            IProgramCrudEventMapper programCrudEventMapper,
            IKafkaProducerFactory producerFactory,
            IKafkaContextProvider contextProvider,
            ILogger logger)
        {
            _assetManager = assetManager;
            _epgAssetManager = epgAssetManager;
            _programCrudEventMapper = programCrudEventMapper;
            _programAssetProducer = producerFactory.Get<string, ProgramAsset>(contextProvider, Partitioner.Murmur2Random);
            _logger = logger;
        }

        public Task PublishCreateEventAsync(long groupId, long epgId, long updaterId)
            => PublishKafkaCreateOrUpdateEventsAsync(
                groupId,
                CrudOperationType.CREATE_OPERATION,
                new[] { epgId },
                updaterId);

        public Task PublishCreateEventsAsync(long groupId, IReadOnlyCollection<long> epgIds, long updaterId)
            => PublishKafkaCreateOrUpdateEventsAsync(groupId, CrudOperationType.CREATE_OPERATION, epgIds, updaterId);

        public Task PublishUpdateEventAsync(long groupId, long epgId, long updaterId)
            => PublishKafkaCreateOrUpdateEventsAsync(
                groupId,
                CrudOperationType.UPDATE_OPERATION,
                new[] { epgId },
                updaterId);

        public Task PublishUpdateEventsAsync(long groupId, IReadOnlyCollection<long> epgIds, long updaterId)
            => PublishKafkaCreateOrUpdateEventsAsync(groupId, CrudOperationType.UPDATE_OPERATION, epgIds, updaterId);

        public Task PublishDeleteEventAsync(long groupId, long epgId, long updaterId)
            => PublishDeleteEventsAsync(groupId, new[] { epgId }, updaterId);

        public Task PublishDeleteEventsAsync(long groupId, IReadOnlyCollection<long> epgIds, long updaterId)
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
                    Operation = CrudOperationType.DELETE_OPERATION,
                    UpdaterId = updaterId
                };

                var task = _programAssetProducer.ProduceAsync(ProgramAsset.GetTopic(), programAssetEvent.GetPartitioningKey(), programAssetEvent);
                publishTasks.Add(task);
            }

            return Task.WhenAll(publishTasks);
        }

        private Task PublishKafkaCreateOrUpdateEventsAsync(
            long groupId,
            long operation,
            IReadOnlyCollection<long> epgIds,
            long updaterId)
        {
            var epgAssets = _epgAssetManager.GetEpgAssets(groupId, epgIds, new List<string> { "*" })
                .ToArray();

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

                var task = PublishKafkaCreateOrUpdateEventAsync(groupId, operation, liveAsset, epgAsset, updaterId);
                publishTasks.Add(task);
            }

            return Task.WhenAll(publishTasks);
        }

        private Task PublishKafkaCreateOrUpdateEventAsync(
            long groupId,
            long operation,
            LiveAsset liveAsset,
            EpgAsset epgAsset,
            long updaterId)
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

            var @event = _programCrudEventMapper.Map(epgAsset, liveAsset, groupId, updaterId, operation);

            return _programAssetProducer.ProduceAsync(ProgramAsset.GetTopic(), @event.GetPartitioningKey(), @event);
        }
    }
}