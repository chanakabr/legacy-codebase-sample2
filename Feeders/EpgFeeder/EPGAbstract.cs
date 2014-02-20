using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpgFeeder
{
    public class EPGAbstract
    {
        protected EPGImplementor m_implementer;

        public EPGImplementor Implementer
        {
            get { return m_implementer; }
            set { m_implementer = value; }
        }

        public virtual Dictionary<DateTime, List<int>> SaveChannel()
        {
            return m_implementer.SaveChannel();
        }
        public virtual void ResetChannelSchedule()
        {
            m_implementer.ResetChannelSchedule();
        }
    }
}
