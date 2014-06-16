using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{
    public class ESMatchQuery : IESTerm
    {
        public enum eMatchQueryType { match = 0, match_phrase = 1, match_phrase_prefix }
        public string Field { get; set; }
        public string Query { get; set; }
        public eTermType eType { get; protected set; }
        public CutWith eOperator { get; set; }
        protected eMatchQueryType eQueryType;

        public ESMatchQuery(eMatchQueryType eMatchQueryType)
        {
            Field = string.Empty;
            eOperator = CutWith.OR;
            Query = string.Empty;
            eQueryType = eMatchQueryType;
            eType = eTermType.MATCH;    
        }

        public bool IsEmpty()
        {
            return (string.IsNullOrEmpty(Field) || string.IsNullOrEmpty(Query)) ? true : false;
        }

        public override string ToString()
        {
            if (this.IsEmpty())
                return string.Empty;

            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("{ \"match\": { ");
            sbQuery.Append(string.Concat("\"", Field, "\":{"));
            sbQuery.AppendFormat("\"query\": \"{0}\", \"operator\": \"{1}\", \"type\": \"{2}\" ", Query, eOperator, eQueryType.ToString());
            sbQuery.Append("}}}");

            return sbQuery.ToString();
        }

    }
}
