using System;
using System.Runtime.CompilerServices;
using DAL.MongoDB;
using LiveToVod.BOL;
using LiveToVod.DAL;
using Microsoft.Extensions.Logging;

namespace LiveToVod.IntegrationTests.Infrastructure
{
    internal static class RepositoryFactory
    {
        private static readonly Random Random = new Random();
        public const long LINEAR_ASSET_ID_1 = 11;
        public const long LINEAR_ASSET_ID_2 = 12;

        public static Tuple<long, Repository> Get(ILogger logger, bool hasPartnerData = true, bool hasLinearAssetsData = true, [CallerMemberName] string callingMethod = "")
        {
            var partnerId = Random.Next();
            var dbName = GetUniqueDbName(partnerId, callingMethod);

            var mongoDbClientFactory = ClientFactoryBuilder.Instance.GetClientFactory(dbName, new MongoConfiguration());
            var repository = new Repository(mongoDbClientFactory, logger);

            if (hasPartnerData)
            {
                var partnerConfig = new LiveToVodPartnerConfiguration(true, 10, "metadataClassifier");
                repository.UpsertPartnerConfiguration(partnerId, partnerConfig, 2);
            }

            if (hasLinearAssetsData)
            {
                var linearAssetConfig1 = new LiveToVodLinearAssetConfiguration(LINEAR_ASSET_ID_1, true, 20);
                repository.UpsertLinearAssetConfiguration(partnerId, linearAssetConfig1, 2);

                var linearAssetConfig2 = new LiveToVodLinearAssetConfiguration(LINEAR_ASSET_ID_2, false);
                repository.UpsertLinearAssetConfiguration(partnerId, linearAssetConfig2, 2);
            }

            return new Tuple<long, Repository>(partnerId, repository);
        }

        private static string GetUniqueDbName(long partnerId, string callingMethod)
        {
            var uniqueDbName = $"{DateTime.UtcNow:HHmmss}_{callingMethod}";
            var truncatedDbName = uniqueDbName.Substring(0, Math.Min(uniqueDbName.Length, 64 - partnerId.ToString().Length - 2)); // max dbname length is 64, but there's partnerId added as prefix to this name

            return truncatedDbName;
        }
    }
}