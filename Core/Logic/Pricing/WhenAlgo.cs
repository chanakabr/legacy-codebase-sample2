using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;

namespace Core.Pricing
{
    [Serializable]
    public class WhenAlgo : IDeepCloneable<WhenAlgo>
    {
        public WhenAlgo()
        {
            m_eAlgoType = WhenAlgoType.EVERY_N_TIMES;
            m_nNTimes = 1;
        }
        
        public WhenAlgo(WhenAlgo other) {
            m_eAlgoType = other.m_eAlgoType;
            m_nNTimes = other.m_nNTimes;
        }
        
        public void Initialize(WhenAlgoType eWhenAlgoType, Int32 nNTimes)
        {
            m_eAlgoType = eWhenAlgoType;
            m_nNTimes = nNTimes;
        }

        //first n times or every n times
        public WhenAlgoType m_eAlgoType;
        public Int32 m_nNTimes;
        public WhenAlgo Clone()
        {
            return new WhenAlgo(this);
        }
    }
}
