using System;
using System.Collections.Generic;
using WebAPI.Models.Catalog;
using WebAPI.Models.Catalog.SearchPriorityGroup;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class SearchPriorityGroupOrderedIdsSetMapper
    {
        public static  IEnumerable<long> GetPriorityGroupIds(this KalturaSearchPriorityGroupOrderedIdsSet model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<long>(model.PriorityGroupIds, "priorityGroupIds");
        }
    }
}