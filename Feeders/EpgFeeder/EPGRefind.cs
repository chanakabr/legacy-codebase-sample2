using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EpgFeeder
{
    public class EPGRefind : EPGAbstract
    {
        public override Dictionary<DateTime, List<int>> SaveChannel()
        {
            return m_implementer.SaveChannel();
        }
        public override void ResetChannelSchedule()
        {
            m_implementer.ResetChannelSchedule();
        }

    }
}
