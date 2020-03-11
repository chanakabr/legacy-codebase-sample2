using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    public class SearchResultsObj
    {
        [DataMember]
        public int n_TotalItems;
        [DataMember]
        public List<SearchResult> m_resultIDs;
    }
}
