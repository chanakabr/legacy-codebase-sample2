using System.Collections.Generic;
using OTT.Lib.MongoDB;

namespace DAL.MongoDB
{
    public interface IClientFactoryBuilder
    {
        IMongoDbClientFactory GetClientFactory(string databaseName, IConnectionStringHelper helper);

        IMongoDbClientFactory GetClientFactory(
            string databaseName,
            Dictionary<string, MongoDbConfiguration.CollectionProperties> collectionProperties,
            IConnectionStringHelper helper);
    }
}