using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class PageDefinition
    {
        public PageDefinition() 
        {
            m_nStartIndex = 0;
            m_nNumberOfItems = 20;
        }

        public void Initialize(Int32 nStartIndex, Int32 nNumberOfItems)
        {
            m_nStartIndex = nStartIndex;
            m_nNumberOfItems = nNumberOfItems;
        }

        public Int32 m_nStartIndex;
        public Int32 m_nNumberOfItems;
    }
}
