using ApiObjects;
using CachingProvider.LayeredCache;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using ApiLogic.EPG;
using ApiObjects.Epg;
using Core.Api;
using Tvinci.Core.DAL;
using System.Data;

namespace Core.GroupManagers
{
    public interface IGroupSettingsManager
    {
        bool IsOpc(int groupId);
        bool IsTvm(int groupId);

        bool DoesGroupUsesTemplates(int groupId);

        EpgFeatureVersion GetEpgFeatureVersion(int partnerId);
        IEnumerable<int> GetPartnersByEpgFeatureVersion(params EpgFeatureVersion[] versionsIn);
    }

    public class GroupSettingsManager : IGroupSettingsManager
    {
        private readonly ILayeredCache _layeredCache;
        private readonly IEpgPartnerConfigurationManager _epgPartnerConfigurationManager;
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<IGroupSettingsManager> _lazy =
            new Lazy<IGroupSettingsManager>(GetGroupSettingsManagerInstance, LazyThreadSafetyMode.PublicationOnly);

        public static IGroupSettingsManager Instance => _lazy.Value;

        private static IGroupSettingsManager GetGroupSettingsManagerInstance() => new GroupSettingsManager(LayeredCache.Instance, EpgPartnerConfigurationManager.Instance);

        public GroupSettingsManager(ILayeredCache layeredCache, IEpgPartnerConfigurationManager epgPartnerConfigurationManager)
        {
            _layeredCache = layeredCache;
            _epgPartnerConfigurationManager = epgPartnerConfigurationManager;
        }

        public bool IsOpc(int groupId) => DoesGroupUsesTemplates(groupId);
        public bool IsTvm(int groupId) => !IsOpc(groupId);

        public bool DoesGroupUsesTemplates(int groupId)
        {
            bool result = false;
            try
            {
                string key = LayeredCacheKeys.GetDoesGroupUsesTemplatesCacheKey(groupId);

                if (!_layeredCache.Get<bool>(key, ref result, DoesGroupUsesTemplates, new Dictionary<string, object>() { { "groupId", groupId } }, groupId,
                                                        LayeredCacheConfigNames.DOES_GROUP_USES_TEMPLATES_CACHE_CONFIG_NAME))
                {
                    _logger.ErrorFormat("Failed getting DoesGroupUsesTemplates from LayeredCache, groupId: {0}", groupId);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed DoesGroupUsesTemplates with groupId: {0}", groupId), ex);
            }

            return result;
        }

        public EpgFeatureVersion GetEpgFeatureVersion(int partnerId)
        {
            var epgV3PartnerConfig = _epgPartnerConfigurationManager.GetEpgV3Configuration(partnerId);
            if (epgV3PartnerConfig.IsEpgV3Enabled) { return EpgFeatureVersion.V3; }

            var epgV2PartnerConfig = _epgPartnerConfigurationManager.GetEpgV2Configuration(partnerId);
            if (epgV2PartnerConfig.IsEpgV2Enabled) { return EpgFeatureVersion.V2; }

            return EpgFeatureVersion.V1;
        }

        public IEnumerable<int> GetPartnersByEpgFeatureVersion(params EpgFeatureVersion[] versionsIn)
        {
            var dt = DAL.ApiDAL.Get_SubGroupsTree();
            if (dt == null) { _logger.Error($"error while getting list of groups from db."); }
            var parentGroupIds = new HashSet<long>();
            if (dt.Rows != null)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var id = (long)row["ID"];
                    var parentId = (long)row["PARENT_GROUP_ID"];
                    if (parentId == 1) { parentGroupIds.Add(id); }
                }
            }

            _logger.Debug($"found list of parentGroupIds:[{string.Join(",", parentGroupIds)}]");

            var groupsThatImplementRequiredEpgVersion = parentGroupIds
                .Select(pId => (int)pId)
                .Where(pId => versionsIn
                .Contains(GetEpgFeatureVersion(pId)))
                .ToList();

            return groupsThatImplementRequiredEpgVersion;

        }

        private static Tuple<bool, bool> DoesGroupUsesTemplates(Dictionary<string, object> funcParams)
        {
            bool res = false;
            bool doesGroupUsesTemplates = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        doesGroupUsesTemplates = CatalogDAL.DoesGroupUsesTemplates(groupId.Value);
                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("DoesGroupUsesTemplates failed params : {0}", funcParams != null ? string.Join(";",
                         funcParams.Select(x => string.Format("key:{0}, value: {1}", x.Key, x.Value.ToString())).ToList()) : string.Empty), ex);
            }

            return new Tuple<bool, bool>(doesGroupUsesTemplates, res);
        }
    }
}
