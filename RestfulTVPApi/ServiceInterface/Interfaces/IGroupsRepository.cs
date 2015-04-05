using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IGroupsRepository
    {
        List<GroupOperator> GetGroupOperators(GetGroupOperatorsRequest request);

        List<GroupRule> GetGroupRules(GetGroupRulesRequest request);
    }
}