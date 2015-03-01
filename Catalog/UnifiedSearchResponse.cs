using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    /// <summary>
    /// Catalog response that holds list of search results and their types
    /// </summary>
    public class UnifiedSearchResponse : BaseResponse
    {
        [DataMember]
        public List<UnifiedSearchResult> SearchResults;
        
        public UnifiedSearchResponse()
        {
            SearchResults = new List<UnifiedSearchResult>();
        }
    }
}