using System;
using System.Collections.Generic;
using ApiObjects;
using ApiObjects.SearchObjects;
using Core.Catalog;
using Core.GroupManagers;
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
            var mockGroupSettingsManager = new Mock<IGroupSettingsManager>();
            mockGroupSettingsManager.Setup(x => x.GetEpgFeatureVersion(partnerId)).Returns(EpgFeatureVersion.V3);

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

            
            Core.Catalog.Searchers.Helper.WrapFilterWithCommittedOnlyTransactionsForEpgV3(partnerId, query.Filter, mockGroupSettingsManager.Object);
            var expected = "{ \"size\": 10000,  \"from\": 0, \"fields\": [\"_id\",\"_index\",\"_type\",\"_score\",\"group_id\",\"media_id\",\"epg_id\",\"name\",\"cache_date\",\"update_date\"], \"query\": { \"filtered\": {\"filter\":{\"and\": [{\"and\": [{\"range\":{\"end_date\":{\"gte\":\"20220622000000\"}}},{\"range\":{\"start_date\":{\"lte\":\"20220623000000\"}}},{\"terms\":{\"epg_channel_id\":[999]}}]},{ \"bool\": { \"should\": [ { \"bool\": { \"must\": [ {\"terms\":{\"_index\":[\"99999_epg_v3\"]}},{ \"bool\": { \"should\": [ { \"bool\": { \"must\": [ {\"has_parent\":{\"parent_type\":\"transaction\",\"query\":{\"match_all\":{}}}},{\"terms\":{\"__documentTransactionalStatus\":[\"INSERTING\"]}}] }},{ \"bool\": { \"must\": [ {\"terms\":{\"__documentTransactionalStatus\":[\"DELETING\"]}}], \"must_not\": [ {\"has_parent\":{\"parent_type\":\"transaction\",\"query\":{\"match_all\":{}}}}] }}] }}] }},{ \"bool\": { \"must_not\": [ {\"terms\":{\"_index\":[\"99999_epg_v3\"]}}] }}] }}]}}}}";

            Console.WriteLine(query.ToString());
            Assert.AreEqual(query.ToString(), expected);
            mockGroupSettingsManager.VerifyAll();
        }
    }
}
