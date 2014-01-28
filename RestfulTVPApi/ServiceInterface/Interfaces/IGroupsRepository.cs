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
        IEnumerable<GroupOperator> GetGroupOperators(InitializationObject initObj, string scope);

        IEnumerable<GroupRule> GetGroupRules(InitializationObject initObj);
    }
}