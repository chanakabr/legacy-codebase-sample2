using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.CrowdsourceItems;
using ApiObjects.CrowdsourceItems.Base;
using ApiObjects.CrowdsourceItems.CbDocs;
using Newtonsoft.Json;
using CouchbaseManager;

namespace DAL
{
    public static class CrowdsourceDAL
    {
        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.CROWDSOURCE, false, true);

        public static int GetLastItemId(int groupId, eCrowdsourceType type, int assetId)
        {
            try
            {
                CrowdsourceJobDoc job = new CrowdsourceJobDoc(groupId, type, assetId);
                {
                    job = cbManager.Get<CrowdsourceJobDoc>(job.Id, true);
                    if (job != null)
                    {
                        return job.LastItemId;
                    }
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
                return cbManager.Set(job.Id, job, 0, true);
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
                if (cbManager.Get<CrowdsourceFeedDoc>(doc.Id) == null)
                {
                    cbManager.Add(doc.Id, doc);
                }

                ulong version;
                CrowdsourceFeedDoc feedDoc = cbManager.GetWithVersion<CrowdsourceFeedDoc>(doc.Id, out version, true);
                if (feedDoc == null)
                {
                    feedDoc = new CrowdsourceFeedDoc(groupId, csItem.Key);
                    cbManager.Add(feedDoc.Id, feedDoc);
                    feedDoc = cbManager.GetWithVersion<CrowdsourceFeedDoc>(doc.Id, out version);
                }
                feedDoc.Items.Insert(0, csItem.Value);
                feedDoc.Items = feedDoc.Items.Take(TCMClient.Settings.Instance.GetValue<int>("crowdsourcer.FEED_NUM_OF_ITEMS")).ToList();

                return cbManager.SetWithVersionWithRetry<CrowdsourceFeedDoc>(feedDoc.Id, feedDoc, version, 10, 1000, 0, true);
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
            CrowdsourceFeedDoc csDoc = cbManager.Get<CrowdsourceFeedDoc>(doc.Id, true);
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
