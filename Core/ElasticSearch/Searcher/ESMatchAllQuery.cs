using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{
    public class ESMatchAllQuery : IESTerm
    {
        private static readonly string MATCH_ALL_ES_QUERY = "{\"match_all\":{}}";

        public ESMatchAllQuery()
        {
            eType = eTermType.MATCH_ALL;
        }

        public eTermType eType
        {
            get;
            protected set;
        }

        public bool IsEmpty()
        {
            return false;
        }

        public override string ToString()
        {
            return MATCH_ALL_ES_QUERY;
        }
    }
}
