using ApiObjects.SearchObjects;
using CachingHelpers;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog
{
    public class ParentalRulesTagsCache : BaseCacheHelper<ParentalRulesTags>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Singleton

        private static ParentalRulesTagsCache instance;

        public static ParentalRulesTagsCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new ParentalRulesTagsCache();
                    }
                }
            }

            return instance;
        }

        #endregion

        protected override ParentalRulesTags BuildValue(params object[] parameters)
        {
            int groupId = (int)parameters[0];
            string siteGuid = (string)parameters[1];

            Dictionary<string, List<string>> mediaParentalRulesTags = null;
            Dictionary<string, List<string>> epgParentalRulesTags = null;

            CatalogLogic.GetParentalRulesTags(groupId, siteGuid, out mediaParentalRulesTags, out epgParentalRulesTags);

            ParentalRulesTags result = new ParentalRulesTags()
            {
                mediaTags = mediaParentalRulesTags,
                epgTags = epgParentalRulesTags
            };

            return result;
        }

        public ParentalRulesTags GetParentalRulesTags(int groupId, string siteGuid)
        {
            string cacheKey = string.Format("{0}_parental_rules_tags_{1}_{2}", version, groupId, siteGuid);
            string mutexName = string.Concat("Group ParentalRulesTags GID_", groupId);

            this.cacheTime = 4;

            return base.Get(cacheKey, mutexName, groupId, siteGuid);   
        }
    }
}
