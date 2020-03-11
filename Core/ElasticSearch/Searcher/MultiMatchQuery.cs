using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{
    public class MultiMatchQuery : IESTerm
    {
        public List<string> Fields { get; protected set; }
        public string Query { get; set; }
        public eTermType eType { get; protected set; }

        public MultiMatchQuery()
        {
            Fields = new List<string>();
            Query = string.Empty;
            eType = eTermType.MULTI_MATCH;
        }

        public bool IsEmpty()
        {
            return Fields.Count == 0 || string.IsNullOrEmpty(Query);
        }

        public override string ToString()
        {
            if (this.IsEmpty())
                return string.Empty;


            for (int i = 0; i < Fields.Count; i++)
            {
                Fields[i] = string.Format("\"{0}\"", Fields[i]);
            }

            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("{ \"multi_match\": { ");
            sbQuery.AppendFormat("\"query\": \"{0}\", \"fields\": [ {1} ], \"type\": \"phrase_prefix\" ", Query, Fields.Aggregate((current, next) => current + "," + next));
            sbQuery.Append("}}");

            return sbQuery.ToString();
        }

    }
}
