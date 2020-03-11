using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class WhenAlgo
    {
        public WhenAlgo()
        {
            m_eAlgoType = WhenAlgoType.EVERY_N_TIMES;
            m_nNTimes = 1;
        }

        public void Initialize(WhenAlgoType eWhenAlgoType, Int32 nNTimes)
        {
            m_eAlgoType = eWhenAlgoType;
            m_nNTimes = nNTimes;
        }

        //first n times or every n times
        public WhenAlgoType m_eAlgoType;
        public Int32 m_nNTimes;
    }
}
