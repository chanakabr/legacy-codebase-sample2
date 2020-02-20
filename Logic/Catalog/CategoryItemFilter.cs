using ApiObjects.Base;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.Catalog
{
    public class CategoryItemFilter : ICrudFilter
    {
        public ApiObjects.SearchObjects.OrderObj OrderBy { get; set; }
    }

    public class CategoryItemByIdInFilter : CategoryItemFilter
    {
        public List<long> IdIn { get; set; }
    }

    public class CategoryItemSearchFilter : CategoryItemFilter
    {
        public string Ksql { get; set; }

        public bool RootOnly { get; set; }
    }
}