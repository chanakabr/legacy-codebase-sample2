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
using RestfulTVPApi.ServiceModel;

namespace RestfulTVPApi.ServiceInterface
{
    public class GroupsRepository : IGroupsRepository
    {
        public List<GroupOperator> GetGroupOperators(GetGroupOperatorsRequest request)
        {
            return ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).GetGroupOperators(request.scope);            
        }

        public List<GroupRule> GetGroupRules(GetGroupRulesRequest request)
        {
            return ServicesManager.ApiApiService(request.GroupID, request.InitObj.Platform).GetGroupRules();
        }
    }
}