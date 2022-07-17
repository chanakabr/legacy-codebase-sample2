using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using OTT.Lib.MongoDB;

namespace DAL.MongoDB
{
    public class ClientFactoryBuilder : IClientFactoryBuilder
    {
        private static readonly Lazy<IClientFactoryBuilder> LazyInstance = new Lazy<IClientFactoryBuilder>(
            () => new ClientFactoryBuilder(TcmConnectionStringHelper.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private static readonly ConcurrentDictionary<string, IMongoDbClientFactory> MongoDbClientFactories
            = new ConcurrentDictionary<string, IMongoDbClientFactory>();
        private static readonly ConcurrentDictionary<string, IMongoDbAdminClientFactory> MongoDbAdminClientFactories
            = new ConcurrentDictionary<string, IMongoDbAdminClientFactory>();

        public static IClientFactoryBuilder Instance => LazyInstance.Value;
        private readonly IConnectionStringHelper _connectionStringHelper;

        public ClientFactoryBuilder(IConnectionStringHelper connectionStringHelper)
        {
            _connectionStringHelper = connectionStringHelper;
        }

        public IMongoDbClientFactory GetClientFactory(string databaseName)
            => GetClientFactory(databaseName, null);

        public IMongoDbClientFactory GetClientFactory(
            string databaseName,
            Dictionary<string, MongoDbConfiguration.CollectionProperties> collectionProperties)
            => MongoDbClientFactories.GetOrAdd(databaseName,
                dbName =>
                {
                    var configuration = new MongoDbConfiguration
                    {
                        ConnectionString = _connectionStringHelper.GetConnectionString()
                    };
                    if (collectionProperties != null)
                    {
                        configuration.CollectionProps = collectionProperties;
                    }

                    return new ServiceCollection()
                        .AddMongoDbClientFactory(configuration, dbName)
                        .BuildServiceProvider()
                        .GetService<IMongoDbClientFactory>();
                });

        public IMongoDbAdminClientFactory GetAdminClientFactory(string databaseName)
            => GetAdminClientFactory(databaseName, null);

        public IMongoDbAdminClientFactory GetAdminClientFactory(
            string databaseName,
            Dictionary<string, MongoDbConfiguration.CollectionProperties> collectionProperties)
        {
            var mongoDbClientFactory = GetClientFactory(databaseName, collectionProperties);

            return MongoDbAdminClientFactories.GetOrAdd(
                databaseName,
                dbName =>
                {
                    var configuration = new MongoDbAdminConfiguration
                    {
                        ConnectionString = _connectionStringHelper.GetConnectionString()
                    };

                    return new ServiceCollection()
                        .AddMongoDbAdminClientFactory(mongoDbClientFactory, configuration)
                        .BuildServiceProvider()
                        .GetService<IMongoDbAdminClientFactory>();
                });
        }
    }
}