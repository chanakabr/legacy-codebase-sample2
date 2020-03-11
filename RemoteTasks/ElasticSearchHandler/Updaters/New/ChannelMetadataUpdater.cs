using ApiObjects;
using ElasticSearch.Common;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using ElasticSearch.Searcher;
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
        private ElasticSearch.Common.ESSerializerV2 esSerializer;
        private ElasticSearch.Common.ElasticSearchApi esApi;

        #endregion

        #region Properties

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        public string ElasticSearchUrl
        {
            get
            {
                if (esApi != null)
                {
                    return esApi.baseUrl;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (esApi != null)
                {
                    esApi.baseUrl = value;
                }
            }
        }

        #endregion

        #region Ctors

        public ChannelMetadataUpdater(int groupId)
        {
            this.groupId = groupId;
            esSerializer = new ElasticSearch.Common.ESSerializerV2();
            esApi = new ElasticSearch.Common.ElasticSearchApi();
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

            if (!esApi.IndexExists(ElasticSearchTaskUtils.GetChannelMetadataIndexName(groupId)))
            {
                log.Error("Error - " + string.Format("Index of type channel for group {0} does not exist", groupId));

                return result;
            }

            ElasticsearchWrapper wrapper = new ElasticsearchWrapper();
            CatalogGroupCache catalogGroupCache = null;

            // Check if group supports Templates
            if (CatalogManager.DoesGroupUsesTemplates(groupId))
            {

                try
                {
                    if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
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
                            result = IndexManager.UpsertChannel(groupId, id);
                            break;
                        }
                    case eAction.Off:
                    case eAction.Delete:
                        {
                            result = IndexManager.DeleteChannel(groupId, id);
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
