using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.ServiceModel;
using System.Text;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.IndexManager.Transaction;
using ApiObjects;
using ApiObjects.SearchObjects;
using CachingProvider;
using CachingProvider.LayeredCache;
using Core.Catalog.Cache;
using Core.GroupManagers;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using ESUtils = ElasticSearch.Common.Utils;

namespace Core.Catalog.Searchers
{
    public static class Helper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static ConcurrentDictionary<string, object> channelFactories = new ConcurrentDictionary<string, object>();

        public static T GetFactoryChannel<T>(string address) where T : class
        {
            string key = typeof(T).ToString();
            T channel = null;

            if (!channelFactories.ContainsKey(address))//channel factory not cached
            {
                ChannelFactory<T> factory = new ChannelFactory<T>(new BasicHttpBinding(), new EndpointAddress(address));
                channelFactories.TryAdd(key, factory);

            }
            object value;
            if (channelFactories.TryGetValue(key, out value))
            {
                channel = ((ChannelFactory<T>)value).CreateChannel();
                ((IClientChannel)channel).Open();
            }

            return channel;
        }

        public static void CloseChannel(IClientChannel channel)
        {
            try
            {
                if (channel != null && channel.State != CommunicationState.Closed)
                {
                    channel.Close();
                }
            }
            catch (Exception ex)
            {
                log.Error("Close channel request - " + string.Format("exception thrown when closing channel"), ex);
                channel.Abort();
            }
        }


        public static void AbortChannel(IClientChannel channel)
        {
            try
            {
                if (channel != null && channel.State != CommunicationState.Closed)
                {
                    channel.Abort();
                }
            }
            catch (Exception ex)
            {
                log.Error("Abort channel request - " + string.Format("exception thrown when aborting channel"), ex);
                channel.Abort();
            }
        }

        public static void WrapFilterWithCommittedOnlyTransactionsForEpgV3(
            int partnerId,
            QueryFilter originalQuery,
            IGroupSettingsManager groupSettingsManager = null,
            IElasticSearchApi esApi = null,
            ILayeredCache layeredCache = null)
        {
            if ((groupSettingsManager ?? GroupSettingsManager.Instance).GetEpgFeatureVersion(partnerId) != EpgFeatureVersion.V3)
            {
                return;
            }

            var epgV3IndexName = GetEpgV3IndexName(partnerId, esApi ?? ElasticSearchApi.Instance, layeredCache ?? LayeredCache.Instance);

            var epgIndexTerm = ESTerms.GetSimpleStringTerm("_index", new[] {epgV3IndexName});
            var hasTransactionParentDocument =
                new ESCustomQuery($"{{\"has_parent\":{{\"parent_type\":\"{NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME}\",\"query\":{{\"match_all\":{{}}}}}}}}");
            var insertingStatusTerm = ESTerms.GetSimpleStringTerm(ESUtils.ES_DOCUMENT_TRANSACTIONAL_STATUS_FIELD_NAME, new[] {eTransactionOperation.INSERTING.ToString()});
            var deletingStatusTerm = ESTerms.GetSimpleStringTerm(ESUtils.ES_DOCUMENT_TRANSACTIONAL_STATUS_FIELD_NAME, new[] {eTransactionOperation.DELETING.ToString()});

            var insertingAndCommitted = BoolQuery.Must(hasTransactionParentDocument, insertingStatusTerm);
            var deletingAndNotCommitted = BoolQuery.MustNot(hasTransactionParentDocument).AddMust(deletingStatusTerm);
            var allCommittedDocuments = BoolQuery.Should(insertingAndCommitted, deletingAndNotCommitted);
            var epgv3CommittedOnly = BoolQuery.Must(epgIndexTerm, allCommittedDocuments);

            var originalFilter = originalQuery.FilterSettings;
            var compositeFilter = new FilterCompositeType(CutWith.AND);
            compositeFilter.AddChild(new ESCustomQuery($"{{{originalFilter.ToString()}}}"));
            compositeFilter.AddChild(BoolQuery.Should(epgv3CommittedOnly, BoolQuery.MustNot(epgIndexTerm)));
            originalQuery.FilterSettings = compositeFilter;
        }

        private static string GetEpgV3IndexName(int partnerId, IElasticSearchApi esApi, ILayeredCache layeredCache)
        {
            var key = LayeredCacheKeys.GetEpgV3IndexAliasBinding(partnerId);
            var invalidationKeys = new List<string>
            {
                LayeredCacheKeys.GetEpgV3IndexAliasBindingInvalidationKey(partnerId)
            };
            var parameters = new Dictionary<string, object>
            {
                {"partnerId", partnerId},
                {"esApi", esApi}
            };

            string epgV3IndexName = null;
            var res = layeredCache.Get(key,
                ref epgV3IndexName,
                GetEpgV3AliasIndexBinding,
                parameters,
                partnerId,
                LayeredCacheConfigNames.GET_EPG_V3_ALIAS_INDEX_BINDING_CONFIGURATION,
                inValidationKeys: invalidationKeys,
                shouldUseAutoNameTypeHandling: false);
            return !res ? null : epgV3IndexName;
        }

        private static Tuple<string, bool> GetEpgV3AliasIndexBinding(Dictionary<string, object> parameters)
        {
            var partnerId = (int)parameters["partnerId"];
            var esApi = (IElasticSearchApi)parameters["esApi"];

            var epgAlias = NamingHelper.GetEpgIndexAlias(partnerId);
            var indices = esApi.ListIndicesByAlias(epgAlias);
            var indexName = indices.FirstOrDefault()?.Name;
            return string.IsNullOrEmpty(indexName) ? new Tuple<string, bool>(null, false) : new Tuple<string, bool>(indexName, true);
        }
    }
}
