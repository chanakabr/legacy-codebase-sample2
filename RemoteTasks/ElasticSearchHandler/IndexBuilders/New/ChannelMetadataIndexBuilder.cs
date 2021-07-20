using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElasticsearchTasksCommon;
using ApiObjects.SearchObjects;
using GroupsCacheManager;
using System.Data;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using KlogMonitorHelper;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json.Linq;
using Core.Catalog;

namespace ElasticSearchHandler.IndexBuilders
{
    public class ChannelMetadataIndexBuilder : AbstractIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly string CHANNEL = "channel";
        protected const string VERSION = "2";

        public ChannelMetadataIndexBuilder(int groupId) : base(groupId)
        {
        }

        public override bool BuildIndex()
        {
            bool result = false;
            ContextData cd = new ContextData();

            bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
            // Check if group supports Templates
            if (doesGroupUsesTemplates)
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
                    log.Error(string.Format("Failed BuildIndex for channels metadatae of groupId: {0} because of CatalogGroupCache error", groupId), ex);
                    return false;
                }

                string newIndexName = _IndexManager.SetupChannelMetadataIndex();

                if (string.IsNullOrEmpty(newIndexName))
                {
                    return false;
                }

                #region Populate Index
                List<Channel> allChannels = null;
                if (doesGroupUsesTemplates)
                {
                    allChannels = ChannelManager.GetGroupChannels(groupId);
                }
                else
                {
                    GroupManager groupManager = new GroupManager();
                    groupManager.RemoveGroup(groupId);
                    Group group = groupManager.GetGroup(groupId);

                    allChannels = groupManager.GetChannels(group.channelIDs.ToList(), groupId);
                }

                if (allChannels != null)
                {
                    _IndexManager.AddChannelsMetadataToIndex(newIndexName, allChannels);
                }

                #endregion

                #region Switch index alias + Delete old indices handling
                _IndexManager.PublishChannelsMetadataIndex(newIndexName, this.SwitchIndexAlias, this.DeleteOldIndices);

                #endregion

                result = true;
            }

            return result;
        }
    }
}
