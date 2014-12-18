using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{
    public class BoolQuery : IESTerm
    {
        
        protected List<IESTerm> m_lMust;
        protected List<IESTerm> m_lShould;

        public eTermType eType { get; protected set; }

        public BoolQuery()
        {
            m_lMust = new List<IESTerm>();
            m_lShould = new List<IESTerm>();
            eType = eTermType.BOOL_QUERY;
        }


        public bool IsEmpty()
        {
            return m_lMust.Count == 0 && m_lShould.Count == 0;
        }

        public void AddChild(IESTerm oChild, CutWith eCutWith)
        {
            if (oChild == null || oChild.IsEmpty() || eCutWith == CutWith.WCF_ONLY_DEFAULT_VALUE)
                return;

            if (eCutWith == CutWith.AND)
            {
                m_lMust.Add(oChild);
            }
            else if (eCutWith == CutWith.OR)
            {
                m_lShould.Add(oChild);
            }
        }

        public override string ToString()
        {
            if (this.IsEmpty())
                return string.Empty;

            StringBuilder sResult = new StringBuilder();
            
            sResult.Append("{ \"bool\": {");

            bool bMustClausesInserted = false;
            if (m_lMust.Count != 0)
            {
                sResult.Append(" \"must\": [ ");
                List<string> sTerms = new List<string>();

                foreach (IESTerm term in m_lMust)
                {
                    if (term != null && !term.IsEmpty())
                    {
                        bMustClausesInserted = true;
                        sTerms.Add(term.ToString());
                    }
                }

                if (sTerms.Count > 0)
                    sResult.Append(sTerms.Aggregate((current, next) => current + "," + next));

                sResult.Append("]");
            }

            if (m_lShould.Count > 0)
            {
                if (bMustClausesInserted == true)
                    sResult.Append(",");

                sResult.Append(" \"should\": [ ");
                List<string> sTerms = new List<string>();

                foreach (IESTerm term in m_lShould)
                {
                    if (term != null && !term.IsEmpty())
                    {
                        sTerms.Add(term.ToString());
                    }
                }

                if (sTerms.Count > 0)
                    sResult.Append(sTerms.Aggregate((current, next) => current + "," + next));

                sResult.Append("]");
            }

            sResult.Append(" }}");

            return sResult.ToString();
        }
    }

}
