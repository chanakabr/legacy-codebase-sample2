using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearchFeeder
{
    public abstract class ElasticSearchBaseImplementor
    {
        public int m_nGroupID { get; protected set; }
        public string m_sQueueName { get; protected set; }
        public bool m_bRebuildIndex { get; set; }

        public ElasticSearchBaseImplementor(int nGroupID, string sQueueName, bool bReload)
        {
            m_nGroupID = nGroupID;
            m_sQueueName = sQueueName;
            m_bRebuildIndex = bReload;
        }

        public abstract void Update(eESFeederType eESFeeder);
    }
}
