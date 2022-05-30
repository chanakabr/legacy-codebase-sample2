using System;
using DAL;

namespace LiveToVod.IntegrationTests.Infrastructure
{
    internal class MongoConfiguration : IConnectionStringHelper
    {
        public string GetConnectionString()
        {
            var isJenkins = Environment.GetEnvironmentVariable("IS_ON_JENKINS")?.ToLower() == "true";
            var connectionString = isJenkins
                ? "mongodb://root:123456@mongo:27017"
                : "mongodb://root:123456@localhost:27017";

            return connectionString;
        }
    }
}