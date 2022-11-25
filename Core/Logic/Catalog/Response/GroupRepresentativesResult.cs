using System.Collections.Generic;
using Core.Catalog.Request;
using Core.Catalog.Response;

namespace ApiLogic.Catalog.Response
{
    public class GroupRepresentativesResult
    {
        public UnifiedSearchRequest OriginalRequest { get; set; }
        public UnifiedSearchResponse SearchResponse { get; set; }
    }
}