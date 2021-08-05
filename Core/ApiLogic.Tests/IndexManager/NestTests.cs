using System;
using ApiLogic.IndexManager.Helpers;
using ApiObjects;
using ApiObjects.Nest;
using ApiObjects.SearchObjects;
using Core.Catalog;
using ElasticSearch.Searcher;
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
            // Build query for getting programs
            var query = new FilteredQuery();
            var filter = new QueryFilter();

            // basic initialization
            query.PageIndex = 0;
            query.PageSize = 0;
            query.ReturnFields.Clear();
            query.AddReturnField("epg_id");
            query.AddReturnField("document_id");

            var composite = new FilterCompositeType(CutWith.AND);
            var terms = new ESTerms("epg_id", new long[]{1,2,3});
            composite.AddChild(terms);
            filter.FilterSettings = composite;
            query.Filter = filter;
            var searchQuery = query.ToString();
            Console.WriteLine();
            
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
