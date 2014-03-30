using RestfulTVPApi.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IGroupsRepository
    {
        List<GroupOperator> GetGroupOperators(GetGroupOperatorsRequest request);

        List<GroupRule> GetGroupRules(GetGroupRulesRequest request);
    }
}