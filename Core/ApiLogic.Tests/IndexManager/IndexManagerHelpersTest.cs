using System;
using System.Collections.Generic;
using ApiObjects;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
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
        delegate void MockGetEpgV3AliasIndexBindingDelegate(
            string key,
            ref string genericParameter,
            Func<Dictionary<string, object>, Tuple<string, bool>> fillObjectMethod,
            Dictionary<string, object> funcParameters,
            int groupId,
            string layeredCacheConfigName,
            List<string> inValidationKeys = null,
            bool shouldUseAutoNameTypeHandling = false);

        [Test]
        public void Should_GenerateCorrectEpgV3Query()
        {
            var partnerId = 99999;
            var mockRepository = new MockRepository(MockBehavior.Strict);
            var mockGroupSettingsManager = mockRepository.Create<IGroupSettingsManager>();
            mockGroupSettingsManager.Setup(x => x.GetEpgFeatureVersion(partnerId)).Returns(EpgFeatureVersion.V3);

            var mockEsApi = mockRepository.Create<IElasticSearchApi>();
            var mockLayeredCache = mockRepository.Create<ILayeredCache>(MockBehavior.Loose);
            const string epgV3IndexName = "custom_epg_index_name";
            mockLayeredCache.Setup(x => x.Get(It.IsAny<string>(),
                    ref It.Ref<string>.IsAny,
                    It.IsAny<Func<Dictionary<string, object>, Tuple<string, bool>>>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<bool>()))
                .Callback(new MockGetEpgV3AliasIndexBindingDelegate((
                    string key,
                    ref string genericParameter,
                    Func<Dictionary<string, object>, Tuple<string, bool>> fillObjectMethod,
                    Dictionary<string, object> funcParameters,
                    int groupId,
                    string layeredCacheConfigName,
                    List<string> inValidationKeys,
                    bool shouldUseAutoNameTypeHandling) =>
                {
                    genericParameter = epgV3IndexName;
                }))
                .Returns(true);

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


            var query = new FilteredQuery
            {
                Filter = new QueryFilter()
                {
                    FilterSettings = filterComposite
                }
            };

            Core.Catalog.Searchers.Helper.WrapFilterWithCommittedOnlyTransactionsForEpgV3(partnerId, query.Filter, mockGroupSettingsManager.Object, mockEsApi.Object, mockLayeredCache.Object);
            var expected = "{ \"size\": 10000,  \"from\": 0, \"fields\": [\"_id\",\"_index\",\"_type\",\"_score\",\"group_id\",\"media_id\",\"epg_id\",\"name\",\"cache_date\",\"update_date\"], \"query\": { \"filtered\": {\"filter\":{\"and\": [{\"and\": [{\"range\":{\"end_date\":{\"gte\":\"20220622000000\"}}},{\"range\":{\"start_date\":{\"lte\":\"20220623000000\"}}},{\"terms\":{\"epg_channel_id\":[999]}}]},{ \"bool\": { \"should\": [ { \"bool\": { \"must\": [ {\"terms\":{\"_index\":[\"custom_epg_index_name\"]}},{ \"bool\": { \"should\": [ { \"bool\": { \"must\": [ {\"has_parent\":{\"parent_type\":\"transaction\",\"query\":{\"match_all\":{}}}},{\"terms\":{\"__documentTransactionalStatus\":[\"INSERTING\"]}}] }},{ \"bool\": { \"must\": [ {\"terms\":{\"__documentTransactionalStatus\":[\"DELETING\"]}}], \"must_not\": [ {\"has_parent\":{\"parent_type\":\"transaction\",\"query\":{\"match_all\":{}}}}] }}] }}] }},{ \"bool\": { \"must_not\": [ {\"terms\":{\"_index\":[\"custom_epg_index_name\"]}}] }}] }}]}}}}";

            Console.WriteLine(query.ToString());
            Assert.AreEqual(query.ToString(), expected);
            mockRepository.VerifyAll();
        }
    }
}
