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
using ApiObjects.SearchObjects;
using CachingProvider;
using Core.Catalog.Cache;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
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
        
        public static void WrapFilterWithCommittedOnlyTransactionsForEpgV3(int partnerId, QueryFilter originalQuery, IElasticSearchApi esApi)
        {
            // TODO: should we use a better cache mechanism here or configure the cache expiration ? 
            // TODO: we can remove this code as the index name for epg v3 has been made static witout any epoch attached to it
            var epgV3IndexNameKey = $"epg_v3_index_name_{partnerId}";
            var epgV3IndexName = MemoryCache.Default.Get(epgV3IndexNameKey)?.ToString();
            if (string.IsNullOrEmpty(epgV3IndexName))
            {            
                var epgAlias = NamingHelper.GetEpgIndexAlias(partnerId);
                var indices = esApi.ListIndicesByAlias(epgAlias);
                epgV3IndexName = indices.First().Name;
                MemoryCache.Default.Set(
                    new CacheItem(epgV3IndexNameKey,epgV3IndexName),
                    new CacheItemPolicy(){AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(60)}
                );
            }

            var epgIndexTerm = ESTerms.GetSimpleStringTerm("_index", new[] { epgV3IndexName });
            var hasTransactionParentDocument = new ESCustomQuery($"{{\"has_parent\":{{\"parent_type\":\"{NamingHelper.EPG_V3_TRANSACTION_DOCUMENT_TYPE_NAME}\",\"query\":{{\"match_all\":{{}}}}}}}}");
            var insertingStatusTerm = ESTerms.GetSimpleStringTerm(ESUtils.ES_DOCUMENT_TRANSACTIONAL_STATUS_FIELD_NAME, new[] { eTransactionOperation.INSERTING.ToString() });
            var deletingStatusTerm = ESTerms.GetSimpleStringTerm(ESUtils.ES_DOCUMENT_TRANSACTIONAL_STATUS_FIELD_NAME, new[] { eTransactionOperation.DELETING.ToString() });
            

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
    }
}
