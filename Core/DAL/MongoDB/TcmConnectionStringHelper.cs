using System;
using System.Threading;
using Phx.Lib.Appconfig;

namespace DAL
{
    public class TcmConnectionStringHelper : IConnectionStringHelper
    {
        private static readonly Lazy<IConnectionStringHelper> LazyInstance = new Lazy<IConnectionStringHelper>(
            () => new TcmConnectionStringHelper(),
            LazyThreadSafetyMode.PublicationOnly
        );

        public static IConnectionStringHelper Instance => LazyInstance.Value;

        public string GetConnectionString()
        {
            // sample of mongoDB connection string -->   mongodb://username:password@hostName:port/?replicaSet=myRepl
            var connectionString = $"mongodb://{ApplicationConfiguration.Current.MongoDBConfiguration.Username.Value}:" +
                $"{ApplicationConfiguration.Current.MongoDBConfiguration.Password.Value}@" +
                $"{ApplicationConfiguration.Current.MongoDBConfiguration.HostName.Value}:" +
                $"{ApplicationConfiguration.Current.MongoDBConfiguration.Port.Value}";

            return !string.IsNullOrEmpty(ApplicationConfiguration.Current.MongoDBConfiguration.replicaSetName.Value)
                ? $"{connectionString}?replicaSet={ApplicationConfiguration.Current.MongoDBConfiguration.replicaSetName.Value}"
                : connectionString;
        }
    }
}