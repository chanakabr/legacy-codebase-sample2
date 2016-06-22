using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            StringBuilder queryString = new StringBuilder();
            
            queryString.Append("{ \"bool\": {");

            bool hasMustClause = false;
            bool hasShouldClause = false;

            if (must.Count != 0)
            {
                queryString.Append(" \"must\": [ ");
                List<string> terms = new List<string>();

                foreach (IESTerm term in must)
                {
                    if (term != null && !term.IsEmpty())
                    {
                        hasMustClause = true;
                        terms.Add(term.ToString());
                    }
                }

                if (terms.Count > 0)
                {
                    queryString.Append(terms.Aggregate((current, next) => current + "," + next));
                }

                queryString.Append("]");
            }

            if (should.Count > 0)
            {
                if (hasMustClause == true)
                    queryString.Append(",");

                queryString.Append(" \"should\": [ ");
                List<string> terms = new List<string>();

                foreach (IESTerm term in should)
                {
                    if (term != null && !term.IsEmpty())
                    {
                        hasShouldClause = true;
                        terms.Add(term.ToString());
                    }
                }

                if (terms.Count > 0)
                    queryString.Append(terms.Aggregate((current, next) => current + "," + next));

                queryString.Append("]");
            }

            if (mustNot.Count > 0)
            {
                if (hasMustClause || hasShouldClause)
                {
                    queryString.Append(",");
                }

                queryString.Append(" \"must_not\": [ ");
                List<string> terms = new List<string>();

                foreach (IESTerm term in mustNot)
                {
                    if (term != null && !term.IsEmpty())
                    {
                        terms.Add(term.ToString());
                    }
                }

                if (terms.Count > 0)
                    queryString.Append(terms.Aggregate((current, next) => current + "," + next));

                queryString.Append("]");
            }

            if (filter != null && !filter.IsEmpty())
            {
                queryString.AppendFormat(", {0}", filter.ToString());
            }

            queryString.Append(" }}");

            return queryString.ToString();
        }
    }

}
