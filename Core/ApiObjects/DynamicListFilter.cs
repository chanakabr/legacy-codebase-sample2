using ApiObjects.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects
{
    public class DynamicListFilter : ICrudFilter
    {
    }

    public class DynamicListnIdInFilter : DynamicListFilter
    {
        public List<long> IdIn { get; set; }
    }

    public class DynamicListSearchFilter : DynamicListFilter
    {
        public DynamicListType? TypeEqual  { get; set; }
        public List<string> ValueIn { get; set; }
    }
}
