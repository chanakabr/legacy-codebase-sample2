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
        public IEnumerable<GroupOperator> GetGroupOperators(InitializationObject initObj, string scope)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupOperators", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                TVPApiModule.Services.ApiApiService _service = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform);

                return _service.GetGroupOperators(scope);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public IEnumerable<GroupRule> GetGroupRules(InitializationObject initObj)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiApiService _service = new ApiApiService(groupID, initObj.Platform);

                return _service.GetGroupRules();
            }
            else
            {
                throw new UnknownGroupException();
            }
        }
    }
}