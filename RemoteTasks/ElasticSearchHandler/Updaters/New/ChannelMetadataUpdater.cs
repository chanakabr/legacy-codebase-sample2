using ApiObjects;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using KlogMonitorHelper;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json.Linq;
using ApiObjects.Response;
using Core.Catalog;

namespace ElasticSearchHandler.Updaters
{
    public class ChannelMetadataUpdater : IElasticSearchUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly string TAG = "tag";

        #region Data Members

        private int groupId;
        private IIndexManager _indexManager;

        #endregion

        #region Properties

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        #endregion

        #region Ctors

        public ChannelMetadataUpdater(int groupId)
        {
            this.groupId = groupId;
            _indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);
        }

        #endregion

        #region IElasticSearchUpdater

        public bool Start()
        {
            bool result = false;

            log.Debug("Info - Start channel update");

            if (this.IDs == null || this.IDs.Count == 0)
            {
                log.Debug("Info - Tag id list empty");
                result = true;

                return result;
            }

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
                    log.Error(string.Format("Failed update index for channels of groupId: {0} because of CatalogGroupCache error", groupId), ex);
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
                            result = _indexManager.UpsertChannel(id);
                            break;
                        }
                    case eAction.Off:
                    case eAction.Delete:
                        {
                            result = _indexManager.DeleteChannel(id);
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