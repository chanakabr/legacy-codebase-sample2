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
using Tvinci.Core.DAL;

namespace Core.GroupManagers
{
    public interface IGroupSettingsManager
    {
        bool IsOpc(int groupId);
        bool IsTvm(int groupId);
        
        bool DoesGroupUsesTemplates(int groupId);

        bool DoesGroupUseNewEpgIngest(int groupId);
    }
    
    public class GroupSettingsManager : IGroupSettingsManager
    {
        private readonly ILayeredCache _layeredCache;
        private readonly IEpgV2PartnerConfigurationManager _epgV2PartnerConfigurationManager;
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<IGroupSettingsManager> _lazy =
            new Lazy<IGroupSettingsManager>(GetGroupSettingsManagerInstance, LazyThreadSafetyMode.PublicationOnly);

        public static IGroupSettingsManager Instance => _lazy.Value;
        
        private static IGroupSettingsManager GetGroupSettingsManagerInstance() => new GroupSettingsManager(LayeredCache.Instance, EpgV2PartnerConfigurationManager.Instance);

        public GroupSettingsManager(ILayeredCache layeredCache, IEpgV2PartnerConfigurationManager epgV2PartnerConfigurationManager)
        {
            _layeredCache = layeredCache;
            _epgV2PartnerConfigurationManager = epgV2PartnerConfigurationManager;
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

        public bool DoesGroupUseNewEpgIngest(int groupId)
        {
            var epgV2PartnerConfig = _epgV2PartnerConfigurationManager.GetConfiguration(groupId);
            
            

            return epgV2PartnerConfig.IsEpgV2Enabled;
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
