using ApiObjects;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
namespace ElasticSearchHandler.Updaters
{
    public class TagUpdater : IElasticSearchUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly string TAG = "tag";

        #region Data Members

        private int groupId;

        #endregion

        #region Properties

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        #endregion

        #region Ctors

        public TagUpdater(int groupId)
        {
            this.groupId = groupId;
        }

        #endregion

        #region IElasticSearchUpdater

        public bool Start()
        {
            bool result = false;

            log.Debug("Info - Start Tag update");

            if (this.IDs == null || this.IDs.Count == 0)
            {
                log.Debug("Info - Tag id list empty");
                result = true;

                return result;
            }

            var indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);
            CatalogGroupCache catalogGroupCache = null;

            // Check if group supports Templates
            if (CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
            {

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
                    log.Error(string.Format("Failed update index for tags of groupId: {0} because of CatalogGroupCache error", groupId), ex);
                    return false;
                }
            }

            foreach (var id in this.IDs)
            {
                switch (this.Action)
                {
                    case eAction.On:
                    case eAction.Update:
                        {
                            var tagValue = CatalogManager.GetTagById(groupId, id);
                            if (!tagValue.HasObject())
                            {
                                result = false;
                                log.ErrorFormat("Update tag with id {0} failed", id);
                            }
                            else
                            {
                                var status = indexManager.UpdateTag(tagValue.Object);
                                if (!status.IsOkStatusCode())
                                {
                                    result = false;
                                    log.ErrorFormat("Update tag with id {0} failed", id);
                                }
                            }

                            break;
                        }
                    case eAction.Off:
                    case eAction.Delete:
                        {
                            var status = indexManager.DeleteTag(id);

                            if (status == null || status.Code != (int)eResponseStatus.OK)
                            {
                                result = false;
                                log.ErrorFormat("Update tag with id {0} failed", id);
                            }

                            break;
                        }
                    default:
                        break;
                }
            }

            return result;
        }

        #endregion
    }
}