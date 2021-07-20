using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using KlogMonitorHelper;
using Core.Catalog.CatalogManagement;
using Core.Catalog;
using ApiObjects;

namespace ElasticSearchHandler.IndexBuilders
{
    public class TagsIndexBuilder : AbstractIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected const string VERSION = "2";

        public TagsIndexBuilder(int groupId) : base(groupId)
        {
        }

        public override bool BuildIndex()
        {
            bool result = false;

            // Check if group supports Templates
            if (CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
            {
                CatalogGroupCache catalogGroupCache;

                try
                {
                    if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling BuildIndex", groupId);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed BuildIndex for tags of groupId: {0} because of CatalogGroupCache error", groupId), ex);
                    return false;
                }

                string newIndexName = _IndexManager.SetupTagsIndex();
                #region Populate Index

                var allTagValues = CatalogManager.Instance.GetAllTagValues(groupId);

                if (allTagValues == null)
                {
                    log.ErrorFormat("Error when getting all tag values for group {0}", groupId);
                    return false;
                }

                if (allTagValues != null)
                {
                    _IndexManager.AddTagsToIndex(newIndexName, allTagValues);
                }

                #endregion

                result = _IndexManager.PublishTagsIndex(newIndexName, this.SwitchIndexAlias, this.DeleteOldIndices);
            }

            return result;
        }

    }
}
