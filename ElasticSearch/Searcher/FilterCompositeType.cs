using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Searcher
{
    /*
     * If you wish to extend the functionality of this class, there is a decorator you may use
     */
    public class FilterCompositeType : BaseFilterCompositeType
    {
        public FilterCompositeType(CutWith eType)
            : base(eType)
        {

        }

        public override bool IsEmpty()
        {
            return m_lComposite.Count == 0 && m_lTerms.Count == 0;
        }

        public override string ToString()
        {
            if (this.IsEmpty())
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("\"{0}\": [", this.m_eType.ToString().ToLower());

            if (m_lComposite.Count > 0)
            {

                for (int i = 0; i < m_lComposite.Count; i++)
                {
                    sb.Append(" {");
                    sb.Append(m_lComposite[i].ToString());
                    sb.Append(" }");
                    if (i < m_lComposite.Count - 1 || m_lTerms.Count > 0)
                    {
                        sb.Append(",");
                    }
                    string s = sb.ToString();

                }
            }
            if (m_lTerms.Count > 0)
            {
                for (int i = 0; i < m_lTerms.Count; i++)
                {
                    sb.Append(m_lTerms[i].ToString());
                    if (i < m_lTerms.Count - 1)
                    {
                        sb.Append(",");
                    }
                }
            }
            sb.Append("]");

            return sb.ToString();
        }
    }
}
