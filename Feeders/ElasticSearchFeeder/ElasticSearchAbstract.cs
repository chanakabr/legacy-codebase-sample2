using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearchFeeder
{
    public class ElasticSearchAbstract
    {
        protected ElasticSearchBaseImplementor m_oImplementer;
        protected eESFeederType m_eFeeder;

        public ElasticSearchAbstract(eESFeederType eFeeder)
        {
            m_eFeeder = eFeeder;
        }

        public ElasticSearchBaseImplementor Implementer
        {
            set { m_oImplementer = value; }
        }

        public void Start()
        {
            m_oImplementer.Update(m_eFeeder);
        }
    }
}
