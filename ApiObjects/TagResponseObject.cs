using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class TagResponseObject
    {
        public TagResponseObject() 
        {
            m_sTagValue = "";
            m_nCount = 0;
        }

        public void Initialize(string sTagValue, Int32 nCount)
        {
            m_sTagValue = sTagValue;
            m_nCount = nCount;
        }

        public string m_sTagValue;
        public Int32 m_nCount;
    }
}
