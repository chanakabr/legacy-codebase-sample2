using System.Collections.Generic;
using OTT.Lib.MongoDB;

namespace DAL.MongoDB
{
    public interface IClientFactoryBuilder
    {
        IMongoDbClientFactory GetClientFactory(string databaseName);
        IMongoDbClientFactory GetClientFactory(
            string databaseName,
            Dictionary<string, MongoDbConfiguration.CollectionProperties> collectionProperties);
        IMongoDbAdminClientFactory GetAdminClientFactory(string databaseName);
        IMongoDbAdminClientFactory GetAdminClientFactory(
            string databaseName,
            Dictionary<string, MongoDbConfiguration.CollectionProperties> collectionProperties);
    }
}