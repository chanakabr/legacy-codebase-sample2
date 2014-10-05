using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.CrowdsourceItems;
using ApiObjects.CrowdsourceItems.Base;
using ApiObjects.CrowdsourceItems.CbDocs;
using CouchbaseWrapper;
using CouchbaseWrapper.DalEntities;
using Newtonsoft.Json;

namespace DAL
{
    public static class CrowdsourceDAL
    {
        private static GenericCouchbaseClient _client = CouchbaseWrapper.CouchbaseManager.GetInstance("crowdsource");
        
        public static int GetLastItemId(int groupId, eCrowdsourceType type, int assetId)
        {
            try
            {
                CrowdsourceJobDoc job = new CrowdsourceJobDoc(groupId, type, assetId);
                if (_client.Exists(job.Id))
                {
                    job = _client.Get<CrowdsourceJobDoc>(job.Id);
                    return job.LastItemId;
                }
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public static bool SetLastItemId(int groupId, eCrowdsourceType type, int assetId, int newItemId)
        {
            try
            {
                CrowdsourceJobDoc job = new CrowdsourceJobDoc(groupId, type, assetId)
                {
                    LastItemId = newItemId
                };
                return _client.Store(job);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool UpdateCsList(int groupId, KeyValuePair<int, BaseCrowdsourceItem> csItem)
        {
            try
            {
                CrowdsourceFeedDoc doc = new CrowdsourceFeedDoc(groupId, csItem.Key);
                if (!_client.Exists(doc.Id))
                {
                    _client.Store(doc);
                }
                CasGetResult<CrowdsourceFeedDoc> casDoc = _client.GetWithCas<CrowdsourceFeedDoc>(doc.Id);
                if (casDoc.OperationResult == eOperationResult.NoError)
                {
                    if (casDoc.Value == null)
                    {
                        casDoc.Value = new CrowdsourceFeedDoc(groupId, csItem.Key);
                        _client.Store(casDoc.Value);
                        casDoc = _client.GetWithCas<CrowdsourceFeedDoc>(doc.Id);
                    }
                    casDoc.Value.Items.Insert(0,csItem.Value);
                    casDoc.Value.Items = casDoc.Value.Items.Take(TCMClient.Settings.Instance.GetValue<int>("FEED_NUM_OF_ITEMS")).ToList();

                    return _client.CasWithRetry(casDoc.Value, casDoc.DocVersion, 10, 1000);
                }
                return casDoc.OperationResult == eOperationResult.NoError;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static List<BaseCrowdsourceItem> GetCsList(int groupId, int language)
        {
            List<BaseCrowdsourceItem> retVal = null;
            CrowdsourceFeedDoc doc = new CrowdsourceFeedDoc(groupId, language);
                CrowdsourceFeedDoc csDoc = _client.Get<CrowdsourceFeedDoc>(doc.Id);
                if (csDoc != null)
                {
                    if (csDoc.Items != null)
                    {
                        retVal = csDoc.Items;
                    }
                }
            return retVal;
        }
    }
}
