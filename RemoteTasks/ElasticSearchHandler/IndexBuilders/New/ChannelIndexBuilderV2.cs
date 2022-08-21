using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ElasticsearchTasksCommon;
using ApiObjects.SearchObjects;
using GroupsCacheManager;
using System.Data;
using System.Threading.Tasks;
using Phx.Lib.Log;
using System.Reflection;
using ApiObjects.Response;

using Core.Catalog.CatalogManagement;
using Core.Catalog;

namespace ElasticSearchHandler.IndexBuilders
{
    public class ChannelIndexBuilderV2 : AbstractIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ChannelIndexBuilderV2(int groupID)
            : base(groupID)
        {
        }

        #region Interface Methods

        public override bool BuildIndex()
        {
            bool result = false;

            LogContextData cd = new LogContextData();

            CatalogGroupCache catalogGroupCache = null;
            Group group = null;
            GroupManager groupManager = new GroupManager();
            bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
            if (doesGroupUsesTemplates)
            {
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling BuildIndex", groupId);
                    return false;
                }
            }
            else
            {
                groupManager.RemoveGroup(groupId);
                group = groupManager.GetGroup(groupId);
                if (group == null)
                {
                    log.ErrorFormat("Could not load group {0} in channel index builder", groupId);
                    return false;
                }
            }

            try
            {
                HashSet<int> channelIds = new HashSet<int>();

                if (!doesGroupUsesTemplates)
                {
                    channelIds = group.channelIDs;
                }

                // insert / update new channels
                result = _IndexManager.AddChannelsPercolatorsToIndex(channelIds, null, shouldCleanupInvalidChannels: true);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while rebuilding channels in index for group = {0}", groupId, ex);
                return false;
            }

            return true;
        }

        #endregion

    }
}
