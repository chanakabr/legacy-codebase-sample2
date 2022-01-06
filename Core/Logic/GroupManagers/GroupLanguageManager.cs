using ApiObjects;
using CachingProvider.LayeredCache;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Tvinci.Core.DAL;

namespace Core.GroupManagers
{
    public static class GroupLanguageManager
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static List<LanguageObj> GetGroupLanguages(int groupId)
        {

            if (GroupSettingsManager.Instance.IsOpc(groupId))
            {
                List<LanguageObj> result = null;
                string key = LayeredCacheKeys.GetGroupLanguagesCacheKey(groupId);
                var getGroupLanguagesMethodParams = new Dictionary<string, object> { { "groupId", groupId } };
                var invalidationKeys = new List<string>();
                if (!LayeredCache.Instance.Get(key, ref result, GetGroupLanguages, getGroupLanguagesMethodParams, groupId, LayeredCacheConfigNames.GET_GROUP_LANGUAGES, invalidationKeys))
                {
                    _logger.ErrorFormat("Failed getting DoesGroupUsesTemplates from LayeredCache, groupId: {0}", groupId);
                }
                return result;
            }
            else
            {
                return GroupsCacheManager.GroupsCache.Instance().GetGroup(groupId).GetLangauges();
            }
        }

        private static Tuple<List<LanguageObj>, bool> GetGroupLanguages(Dictionary<string, object> arg)
        {
            var languages = CatalogDAL.GetGroupLanguages((int)arg["groupId"]);
            return Tuple.Create(languages, true);
        }
    }
}
