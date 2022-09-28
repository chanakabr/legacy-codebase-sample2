using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Thrift.Protocol;

namespace ElasticSearch.Searcher
{
    public class BoolQuery : IESTerm
    {
        
        protected List<IESTerm> must;
        protected List<IESTerm> should;
        protected List<IESTerm> mustNot;
        public eTermType eType { get; protected set; }
        public QueryFilter filter;

        public bool isNot
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        public static BoolQuery Should(params IESTerm[] terms)
        {
            var q = new BoolQuery();
            foreach (var esTerm in terms)
            {
                q.AddShould(esTerm);
            }
            
            return q;
        }
        
        public static BoolQuery Must(params IESTerm[] terms)
        {
            var q = new BoolQuery();
            foreach (var esTerm in terms)
            {
                q.AddMust(esTerm);
            }
            
            return q;
        }
        
        public static BoolQuery MustNot(params IESTerm[] terms)
        {
            var q = new BoolQuery();
            foreach (var esTerm in terms)
            {
                q.AddMustNot(esTerm);
            }
            
            return q;
        }
        
        public BoolQuery()
        {
            must = new List<IESTerm>();
            should = new List<IESTerm>();
            mustNot = new List<IESTerm>();
            eType = eTermType.BOOL_QUERY;
            filter = new QueryFilter();
        }


        public bool IsEmpty()
        {
            return must.Count == 0 && should.Count == 0 && mustNot.Count == 0 && (filter == null || filter.IsEmpty());
        }

        public void AddChild(IESTerm oChild, CutWith eCutWith)
        {
            if (oChild == null || oChild.IsEmpty() || eCutWith == CutWith.WCF_ONLY_DEFAULT_VALUE)
                return;

            if (eCutWith == CutWith.AND)
            {
                must.Add(oChild);
            }
            else if (eCutWith == CutWith.OR)
            {
                should.Add(oChild);
            }
        }
        
        public BoolQuery AddShould(IESTerm term)
        {
            should.Add(term);
            return this;
        }

        public BoolQuery AddMustNot(IESTerm term)
        {
            mustNot.Add(term);
            return this;
        }

        public BoolQuery AddMust(IESTerm term)
        {
            must.Add(term);
            return this;
        }


        public void AddNot(IESTerm child)
        {
            if (child == null || child.IsEmpty())
            {
                return;
            }

            mustNot.Add(child);
        }

        public override string ToString()
        {
            if (this.IsEmpty())
                return string.Empty;

            var queryString = new StringBuilder();
            
            queryString.Append("{ \"bool\": {");

            var hasMustClause = false;
            var hasShouldClause = false;

            if (must.Count != 0)
            {
                queryString.Append(" \"must\": [ ");
                var terms = GetNonEmptyTerms(must);
                hasMustClause = terms.Any();

                if (hasMustClause) { queryString.Append(terms.Aggregate((current, next) => current + "," + next)); }

                queryString.Append("]");
            }

            if (should.Count > 0)
            {
                if (hasMustClause == true)
                    queryString.Append(",");

                queryString.Append(" \"should\": [ ");
                var terms = GetNonEmptyTerms(should);
                hasShouldClause = terms.Any();
                if (hasShouldClause) { queryString.Append(terms.Aggregate((current, next) => current + "," + next)); }

                queryString.Append("]");
            }

            if (mustNot.Count > 0)
            {
                if (hasMustClause || hasShouldClause) { queryString.Append(","); }

                queryString.Append(" \"must_not\": [ ");
                var terms = GetNonEmptyTerms(mustNot);
                if (terms.Count > 0) { queryString.Append(terms.Aggregate((current, next) => current + "," + next)); }
                queryString.Append("]");
            }

            if (filter != null && !filter.IsEmpty())
            {
                queryString.AppendFormat(", {0}", filter.ToString());
            }

            queryString.Append(" }}");

            return queryString.ToString();
        }

        private List<string> GetNonEmptyTerms(List<IESTerm> source)
        {
            var terms = new List<string>();

            foreach (var term in source)
            {
                if (term != null && !term.IsEmpty())
                {
                    terms.Add(term.ToString());
                }
            }

            return terms;
        }
    }

}
