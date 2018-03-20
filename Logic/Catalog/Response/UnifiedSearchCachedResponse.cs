using Catalog.Response;
using Core.Catalog.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.Catalog.Response
{
    public class UnifiedSearchCachedResponse
    {
        public List<UnifiedSearchResult> SearchResults { get; set; }

        public int TotalItems { get; set; }

        public List<AggregationsResult> AggregationsResult { get; set; }

        public ApiObjects.Response.Status Status { get; set; }
    }
}
