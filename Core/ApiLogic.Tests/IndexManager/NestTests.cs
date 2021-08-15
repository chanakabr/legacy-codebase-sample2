using System;
using System.Collections.Generic;
using ApiLogic.IndexManager.Helpers;
using ApiObjects;
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
            /*// Build query for getting programs
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
            var searchQuery = query.ToString();*/
            
            /*string type = "epg";

            var query = new FilteredQuery(true);

            var channelTerm = new ESTerm(true) {Key = "epg_channel_id", Value = "11111"};

            var endDateRange = new ESRange(false, "end_date", eRangeComp.LTE, DateTime.Now.AddDays(1).ToString("yyyyMMMMdd"));
            var startDateRange = new ESRange(false, "start_date", eRangeComp.GTE, DateTime.Now.ToString("yyyyMMMMdd"));

            var filterCompositeType = new FilterCompositeType(CutWith.AND);
            filterCompositeType.AddChild(endDateRange);
            filterCompositeType.AddChild(startDateRange);
            filterCompositeType.AddChild(channelTerm);

            query.Filter = new QueryFilter()
            {
                FilterSettings = filterCompositeType
            };

            query.ReturnFields.Clear();
            query.AddReturnField("document_id");
            query.AddReturnField("epg_id");
            var esOrderObjs = new List<ESOrderObj>();
            esOrderObjs.Add(new ESOrderObj() { m_eOrderDir = OrderDir.ASC, m_sOrderValue = "start_date" });
            esOrderObjs.Add(new ESOrderObj() { m_eOrderDir = OrderDir.DESC, m_sOrderValue = "end_date" });

            
            foreach (var item in esOrderObjs)
            {
                query.ESSort.Add(item);
            }
            
            // get the epg document ids from elasticsearch
            var searchQuery = query.ToString();*/
            
            
            
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
