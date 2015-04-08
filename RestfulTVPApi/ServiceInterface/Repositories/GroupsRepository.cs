using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestfulTVPApi.ServiceModel;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Clients.Utils;

namespace RestfulTVPApi.ServiceInterface
{
    public class GroupsRepository : IGroupsRepository
    {
        public List<GroupOperator> GetGroupOperators(GetGroupOperatorsRequest request)
        {
            return ClientsManager.ApiClient().GetGroupOperators(request.scope);            
        }

        public List<GroupRule> GetGroupRules(GetGroupRulesRequest request)
        {
            return ClientsManager.ApiClient().GetGroupRules();
        }
    }
}