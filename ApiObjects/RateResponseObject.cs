using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class RateResponseObject
    {
        public RateResponseObject()
        {
            m_oMediaStatistics = null;
            m_oStatus = null;
        }
        public void Initialize(MediaStatistics oMediaStatistics, GenericWriteResponse oStatus)
        {
            m_oMediaStatistics = oMediaStatistics;
            m_oStatus = oStatus;
        }

        public MediaStatistics m_oMediaStatistics;
        public GenericWriteResponse m_oStatus;
    }
}
