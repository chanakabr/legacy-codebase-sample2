using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{
    public class ESMatchQuery : IESTerm
    {
        public enum eMatchQueryType
        {
            phrase = 1,
            match_phrase_prefix = 2
        }

        public string Field
        {
            get;
            set;
        }
        public string Query
        {
            get;
            set;
        }
        public eTermType eType
        {
            get;
            protected set;
        }
        public CutWith eOperator
        {
            get;
            set;
        }

        protected eMatchQueryType? eQueryType;

        public ESMatchQuery(eMatchQueryType? eMatchQueryType = null)
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
            sbQuery.AppendFormat("\"query\": \"{0}\", \"operator\": \"{1}\" ", Query, eOperator);

            if (eQueryType != null && eQueryType.HasValue)
            {
                sbQuery.AppendFormat(", \"type\": \"{0}\" ", eQueryType.Value.ToString());
            }

            sbQuery.Append("}}}");

            return sbQuery.ToString();
        }
    }

    public class ESFuzzyQuery : IESTerm
    {
        public string Field { get; set; }
        public string value { get; set; }
        public string fuzziness { get; private set; }
        public int max_expansions { get; private set; }
        public int prefix_length { get; private set; }
        public bool transpositions { get; private set; }
        public string rewrite { get; private set; }

        public eTermType eType
        {
            get;
            protected set;
        }
        public CutWith eOperator
        {
            get;
            set;
        }

        public ESFuzzyQuery(string _field, string _value)
        {
            this.Field = _field;
            this.value = _value;
            this.fuzziness = "AUTO";
            this.max_expansions = 50;
            this.prefix_length = 0;
            this.transpositions = true;
            this.rewrite = "constant_score";
            eOperator = CutWith.OR;
            eType = eTermType.MATCH;
        }

        public bool IsEmpty()
        {
            return (string.IsNullOrEmpty(Field) || string.IsNullOrEmpty(value));
        }

        public override string ToString()
        {
            if (this.IsEmpty())
                return string.Empty;

            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append("{ \"fuzzy\": { ");
            sbQuery.Append(string.Concat("\"", Field, "\":{"));
            sbQuery.AppendFormat("\"value\": \"{0}\", \"fuzziness\": \"{1}\" ", value, fuzziness);
            sbQuery.AppendFormat(", \"max_expansions\": {0}, \"prefix_length\": {1} ", max_expansions, prefix_length);
            sbQuery.AppendFormat(", \"transpositions\": {0}, \"rewrite\": \"{1}\" ", transpositions.ToString().ToLower(), rewrite);

            sbQuery.Append("}}}");

            return sbQuery.ToString();
        }
    }
}
