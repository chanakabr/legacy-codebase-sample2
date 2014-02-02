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
        List<GroupOperator> GetGroupOperators(InitializationObject initObj, string scope);

        List<GroupRule> GetGroupRules(InitializationObject initObj);
    }
}