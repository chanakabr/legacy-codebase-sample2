using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Financial
{
    public class ContractRange
    {
        //private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Int32 m_nContractRangeId;
        public Int32 m_nValueRangeMin;
        public Int32 m_nValueRangeMax;
        public ValueRangeType m_nValueRangeType;
        public StartCountSince m_eStartCountSince;
        

        public ContractRange()
        {
            m_nContractRangeId = 0;
            m_nValueRangeMin = 0;
            m_nValueRangeMax = 0;
            m_nValueRangeType = ValueRangeType.Default;
            m_eStartCountSince = StartCountSince.Default;
        }     
    } 
}
