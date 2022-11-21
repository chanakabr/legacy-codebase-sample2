using System.Collections.Generic;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using Core.Pricing;

namespace ApiLogic.Pricing.Handlers
{
    public interface IPpvManager
    {
        GenericListResponse<PPVModule> GetPPVModules(ContextData contextData, List<long> PppvModuleIds = null,
            bool shouldShrink = false, int? couponGroupIdEqual = null, bool alsoInActive = false,
            PPVOrderBy orderBy = PPVOrderBy.NameAsc, int pageIndex = 0, int pageSize = 30, bool shouldIgnorePaging = true);
    }
}