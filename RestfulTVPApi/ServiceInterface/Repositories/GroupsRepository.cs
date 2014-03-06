using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Helper;
using TVPApiModule.Objects;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public class GroupsRepository : IGroupsRepository
    {
        public List<GroupOperator> GetGroupOperators(InitializationObject initObj, string scope)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupOperators", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ApiApiService(groupID, initObj.Platform).GetGroupOperators(scope);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public List<GroupRule> GetGroupRules(InitializationObject initObj)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                return ServicesManager.ApiApiService(groupID, initObj.Platform).GetGroupRules();
            }
            else
            {
                throw new UnknownGroupException();
            }
        }
    }
}