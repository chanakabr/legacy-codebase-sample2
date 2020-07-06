using System;
using System.Collections.Generic;
using System.Linq;
using ApiObjects.CrowdsourceItems;
using ApiObjects.CrowdsourceItems.Base;
using ApiObjects.CrowdsourceItems.CbDocs;
using Newtonsoft.Json;
using CouchbaseManager;
using ConfigurationManager;

namespace DAL
{
    public static class CrowdsourceDAL
    {
        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.CROWDSOURCE, false, true);

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
