using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DAL.MongoDB;
using LiveToVod.BOL;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using OTT.Lib.MongoDB;
using Phx.Lib.Log;

namespace LiveToVod.DAL
{
    public class Repository : IRepository
    {
        private static readonly Lazy<Repository> Lazy = new Lazy<Repository>(
            () => new Repository(
                ClientFactoryBuilder.Instance.GetClientFactory(DatabaseProperties.DATABASE),
                ClientFactoryBuilder.Instance.GetAdminClientFactory(DatabaseProperties.DATABASE)),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IMongoDbClientFactory _mongoDbClientFactory;
        private readonly IMongoDbAdminClientFactory _mongoDbAdminClientFactory;
        private readonly ILogger _logger;

        public static IRepository Instance => Lazy.Value;
        
        public Repository(IMongoDbClientFactory mongoDbClientFactory, IMongoDbAdminClientFactory mongoDbAdminClientFactory)
            : this(mongoDbClientFactory, mongoDbAdminClientFactory, new KLogger(nameof(Repository)))
        {
        }

        public Repository(IMongoDbClientFactory mongoDbClientFactory, IMongoDbAdminClientFactory mongoDbAdminClientFactory, ILogger logger)
        {
            _mongoDbClientFactory = mongoDbClientFactory;
            _mongoDbAdminClientFactory = mongoDbAdminClientFactory;
            _logger = logger;
        }

        public IEnumerable<long> GetPartnerIds()
            => _mongoDbAdminClientFactory
                .NewMongoDbAdminClient(_logger)
                .GetPartnerDbClients()
                .Select(x => x.PartnerId)
                .ToList();

        public LiveToVodPartnerConfiguration GetPartnerConfiguration(long partnerId)
        {
            var client = _mongoDbClientFactory.NewMongoDbClient((int)partnerId, _logger);
            var data = client
                .Find<LiveToVodPartnerConfigurationData>(DatabaseProperties.PARTNER_CONFIGURATIONS_COLLECTION, f => f.Eq(x => x.Id, LiveToVodPartnerConfigurationData.PARTNER_CONFIG_DOCUMENT_ID))
                .SingleOrDefault();
            var result = Mapper.Map(data);

            return result;
        }

        public IEnumerable<LiveToVodLinearAssetConfiguration> GetLinearAssetConfigurations(long partnerId)
        {
            var client = _mongoDbClientFactory.NewMongoDbClient((int)partnerId, _logger);
            var data = client
                .Find<LiveToVodLinearAssetConfigurationData>(DatabaseProperties.LINEAR_ASSET_CONFIGURATIONS_COLLECTION, f => f.Empty);
            var result = data.Select(Mapper.Map);

            return result;
        }

        public LiveToVodLinearAssetConfiguration GetLinearAssetConfiguration(long partnerId, long linearAssetId)
        {
            var client = _mongoDbClientFactory.NewMongoDbClient((int)partnerId, _logger);
            var data = client
                .Find<LiveToVodLinearAssetConfigurationData>(DatabaseProperties.LINEAR_ASSET_CONFIGURATIONS_COLLECTION, f => f.Eq(x => x.LinearAssetId, linearAssetId))
                .SingleOrDefault();
            var result = Mapper.Map(data);

            return result;
        }

        public bool UpsertPartnerConfiguration(long partnerId, LiveToVodPartnerConfiguration config, long updaterId)
        {
            var data = Mapper.Map(config, updaterId);
            var client = _mongoDbClientFactory.NewMongoDbClient((int)partnerId, _logger);
            var result = client.UpdateOne<LiveToVodPartnerConfigurationData>(
                DatabaseProperties.PARTNER_CONFIGURATIONS_COLLECTION,
                f => f.Eq(x => x.Id, LiveToVodPartnerConfigurationData.PARTNER_CONFIG_DOCUMENT_ID),
                u => GetUpdateDefinition(data, u),
                new MongoDbUpdateOptions { IsUpsert = true });

            if (result.MatchedCount > 1)
            {
                _logger.LogError($"There have been found {result.MatchedCount} {nameof(LiveToVodPartnerConfiguration)}'s documents in the database: {nameof(partnerId)}={partnerId}.");
            }

            var isInserted = !string.IsNullOrEmpty(result.UpsertedId);
            var isUpdated = result.ModifiedCount == 1;

            return isInserted || isUpdated;
        }

        public bool UpsertLinearAssetConfiguration(long partnerId, LiveToVodLinearAssetConfiguration config, long updaterId)
        {
            var data = Mapper.Map(partnerId, config, updaterId);
            var client = _mongoDbClientFactory.NewMongoDbClient((int)partnerId, _logger);
            var result = client.UpdateOne<LiveToVodLinearAssetConfigurationData>(
                DatabaseProperties.LINEAR_ASSET_CONFIGURATIONS_COLLECTION,
                f => f.Eq(x => x.LinearAssetId, config.LinearAssetId),
                u => GetUpdateDefinition(data, u),
                new MongoDbUpdateOptions { IsUpsert = true });

            if (result.MatchedCount > 1)
            {
                _logger.LogError($"There have been found {result.MatchedCount} {nameof(LiveToVodLinearAssetConfiguration)}'s documents in the database: {nameof(partnerId)}={partnerId}, {nameof(config.LinearAssetId)}={config.LinearAssetId}.");
            }

            var isInserted = !string.IsNullOrEmpty(result.UpsertedId);
            var isUpdated = result.ModifiedCount == 1;

            return isInserted || isUpdated;
        }

        private static UpdateDefinition<LiveToVodPartnerConfigurationData> GetUpdateDefinition(LiveToVodPartnerConfigurationData data, UpdateDefinitionBuilder<LiveToVodPartnerConfigurationData> updateBuilder)
        {
            var updateDefinition = updateBuilder
                .Set(x => x.IsLiveToVodEnabled, data.IsLiveToVodEnabled)
                .Set(x => x.RetentionPeriodDays, data.RetentionPeriodDays)
                .Set(x => x.MetadataClassifier, data.MetadataClassifier)
                .Set(x => x.LastUpdaterId, data.LastUpdaterId);

            return updateDefinition;
        }

        private static UpdateDefinition<LiveToVodLinearAssetConfigurationData> GetUpdateDefinition(LiveToVodLinearAssetConfigurationData data, UpdateDefinitionBuilder<LiveToVodLinearAssetConfigurationData> updateBuilder)
        {
            var updateDefinition = updateBuilder
                .Set(x => x.IsLiveToVodEnabled, data.IsLiveToVodEnabled)
                .Set(x => x.RetentionPeriodDays, data.RetentionPeriodDays)
                .Set(x => x.LastUpdaterId, data.LastUpdaterId);

            return updateDefinition;
        }
    }
}