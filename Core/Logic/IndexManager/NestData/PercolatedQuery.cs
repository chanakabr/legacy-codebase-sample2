using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.IndexManager.NestData
{
    public class PercolatedQuery
    {
        [Percolator()]
        public QueryContainer Query { get; set; }
    }
}
