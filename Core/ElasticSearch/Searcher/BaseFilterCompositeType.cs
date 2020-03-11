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
    public abstract class BaseFilterCompositeType
    {
        protected CutWith m_eType;
        protected List<BaseFilterCompositeType> m_lComposite;
        protected List<IESTerm> m_lTerms;

        public BaseFilterCompositeType()
        {
            m_eType = CutWith.AND;
            m_lComposite = new List<BaseFilterCompositeType>();
            m_lTerms = new List<IESTerm>();
        }

        public BaseFilterCompositeType(CutWith eType)
        {
            m_eType = eType;
            m_lComposite = new List<BaseFilterCompositeType>();
            m_lTerms = new List<IESTerm>();
        }

        public virtual void AddChild(BaseFilterCompositeType child)
        {
            if (child == null || child.IsEmpty())
                return;

            m_lComposite.Add(child);
        }

        public virtual void AddChild(IESTerm child)
        {
            if (child == null || child.IsEmpty())
                return;

            m_lTerms.Add(child);
        }

        public virtual bool IsEmpty()
        {
            return (m_lComposite.Count == 0 && m_lTerms.Count == 0);
        }

        public abstract string ToString();
    }
}
