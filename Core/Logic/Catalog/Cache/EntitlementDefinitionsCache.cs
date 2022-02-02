using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingHelpers;
using Core.Catalog.Request;
using GroupsCacheManager;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog
{
    public class EntitlementDefinitionsCache : BaseCacheHelper<EntitlementSearchDefinitions>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Singleton

        private static EntitlementDefinitionsCache instance;

        public static EntitlementDefinitionsCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new EntitlementDefinitionsCache();
                    }
                }
            }

            return instance;
        }

        #endregion

        protected override EntitlementSearchDefinitions BuildValue(params object[] parameters)
        {
            int groupId = (int)parameters[0];
            UnifiedSearchDefinitions definitions = (UnifiedSearchDefinitions)parameters[1];
            BaseRequest request = (BaseRequest)parameters[2];
            Group group = (Group)parameters[3];

            UnifiedSearchDefinitionsBuilder.BuildEntitlementSearchDefinitions(definitions, request, groupId, group);

            return definitions.entitlementSearchDefinitions;
        }

        public EntitlementSearchDefinitions GetEntitlementSearchDefinitions(UnifiedSearchDefinitions definitions,
            BaseRequest request,
            int groupId, 
            Group group,
            eEntitlementSearchType type)
        {
            string cacheKey = string.Format("{0}_entitlement_search_definitions_{1}_{2}_{3}", version, groupId, request.m_sSiteGuid, type.ToString());
            this.cacheTime = 4;
            EntitlementSearchDefinitions result = null;

            // If it is the first page, always rebuild value and set it
            if (request.m_nPageIndex == 0)
            {
                result = BuildValue(groupId, definitions, request, group);
                this.cacheService.Set(cacheKey, new CachingProvider.BaseModuleCache(result), cacheTime);
            }
            else
            {
                // For second page onwards, use cache
                result = base.Get(cacheKey, groupId, definitions, request, group);
            }

            return result;
        }
    }
}
