using System;
using ApiLogic.IndexManager.Helpers;
using ApiObjects;
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
            
        }
  
        
        
        
        public void TEstAttrWithLowLevel()
        {
            /*var elasticClient = NESTFactory.GetInstance(ApplicationConfiguration.Current);
            var partnerId = IndexManagerMockDataCreator.GetRandomPartnerId();
            
            IndexName indexName = $"{partnerId}_gil";
            var epgCb = new EpgCB();
            epgCb.Name = "lamovie";
            epgCb.Language = "rus";
            
            var epgCb2 = new EpgCB();
            epgCb2.Name = "lamovie";
            epgCb2.Language = "heb";
            var buildEpg = new ElasticSearchNestDataBuilder().BuildEpg(epgCb, epgCb.Language, isOpc: true);
            
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CustomResolver(epgCb.Language)
            };
            var json = JsonConvert.SerializeObject(buildEpg, settings);
            var indexResponse = elasticClient.Index(json, x => x.Index(indexName));
            
            var index = $"{partnerId}_gil";
            var stringResponse = elasticClient.LowLevel.Index<StringResponse>(index, PostData.String(json));*/
        }

    }
}
