using System;
using ApiLogic.IndexManager.Helpers;
using ApiObjects;
using ApiObjects.Nest;
using Nest;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ApiLogic.Tests.IndexManager
{
    [TestFixture]
    public class NestTests
    {
        private ElasticClient _client;

        [SetUp]
        public void Setup()
        {
             //es7 client
             _client = new ElasticClient(new Uri("http://localhost:9200"));
        }
        
        [Test]
        public void TestCreateIndex()
        {
            var epgCb = new EpgCB();
            epgCb.Name = "lamovie";
            epgCb.Language = "rus";
            var buildEpg = new ElasticSearchNestDataBuilder().BuildEpg(epgCb, epgCb.Language, isOpc: true);
            
            if (buildEpg != null)
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CustomResolver(epgCb.Language)
                };

                string json = JsonConvert.SerializeObject(buildEpg, settings);
            }
        }
        
    }
}
