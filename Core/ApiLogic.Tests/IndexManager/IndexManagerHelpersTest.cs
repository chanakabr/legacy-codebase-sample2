using System;
using System.Collections.Generic;
using ApiObjects.SearchObjects;
using Core.Catalog;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using Moq;
using NUnit.Framework;
using ESUtils = ElasticSearch.Common.Utils;

namespace ApiLogic.Tests.IndexManager
{
    public class IndexManagerHelpersTest
    {

        [Test]
        public void Should_GenerateCorrectEpgV3Query()
        {
            var partnerId = 99999;
            var epgIndexName = "test_index_1";
            var esApiMock = new Mock<IElasticSearchApi>();
            esApiMock.Setup(mock => mock.ListIndicesByAlias(It.IsAny<string>())).Returns(new List<ESIndex>() { new ESIndex() { Name = epgIndexName } });
            
            
            var fromDate = new DateTime(2022, 06, 22);
            var toDate = new DateTime(2022, 06, 23);
            var channelId = 999;
            var minimumRange = new ESRange(false, "end_date", eRangeComp.GTE, fromDate.ToString(ESUtils.ES_DATE_FORMAT));
            var maximumRange = new ESRange(false, "start_date", eRangeComp.LTE, toDate.ToString(ESUtils.ES_DATE_FORMAT));
            var channelFilter = ESTerms.GetSimpleNumericTerm("epg_channel_id", new[] { channelId });
            var filterComposite = new FilterCompositeType(CutWith.AND);
            filterComposite.AddChild(minimumRange);
            filterComposite.AddChild(maximumRange);
            filterComposite.AddChild(channelFilter);

            
            var query = new FilteredQuery();
            query.Filter = new QueryFilter()
            {
                FilterSettings = filterComposite
            };

            
            Core.Catalog.Searchers.Helper.WrapFilterWithCommittedOnlyTransactionsForEpgV3(partnerId, query.Filter, esApiMock.Object);
            var expected = "{ \"size\": 10000,  \"from\": 0, \"fields\": [\"_id\",\"_index\",\"_type\",\"_score\",\"group_id\",\"media_id\",\"epg_id\",\"name\",\"cache_date\",\"update_date\"], \"query\": { \"filtered\": {\"filter\":{\"and\": [{\"and\": [{\"range\":{\"end_date\":{\"gte\":\"20220622000000\"}}},{\"range\":{\"start_date\":{\"lte\":\"20220623000000\"}}},{\"terms\":{\"epg_channel_id\":[999]}}]},{ \"bool\": { \"should\": [ { \"bool\": { \"must\": [ {\"terms\":{\"_index\":[\"test_index_1\"]}},{ \"bool\": { \"should\": [ { \"bool\": { \"must\": [ {\"has_parent\":{\"parent_type\":\"transaction\",\"query\":{\"match_all\":{}}}},{\"terms\":{\"__documentTransactionalStatus\":[\"INSERTING\"]}}] }},{ \"bool\": { \"must\": [ {\"terms\":{\"__documentTransactionalStatus\":[\"DELETING\"]}}], \"must_not\": [ {\"has_parent\":{\"parent_type\":\"transaction\",\"query\":{\"match_all\":{}}}}] }}] }}] }},{ \"bool\": { \"must_not\": [ {\"terms\":{\"_index\":[\"test_index_1\"]}}] }}] }}]}}}}";

            Console.WriteLine(query.ToString());
            Assert.AreEqual(query.ToString(), expected);
            
            //calling second time to verify that index name is taken from memory cache...
            Core.Catalog.Searchers.Helper.WrapFilterWithCommittedOnlyTransactionsForEpgV3(partnerId, query.Filter, esApiMock.Object);
            esApiMock.Verify(mock => mock.ListIndicesByAlias(It.IsAny<string>()), Times.Once());
        }
    }
}