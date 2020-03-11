using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class ChannelRequestObject
    {
        public ChannelRequestObject() 
        {
            m_nChannelID = 0;
            m_oPageDef = null;
            m_oOrderBy = null;
        }

        public void Initialize(Int32 nChannelID, PageDefinition thePageDef , SearchOrderByObject[] oOrderBy)
        {
            m_nChannelID = nChannelID;
            m_oPageDef = thePageDef;
            m_oOrderBy = oOrderBy;
        }

        public PageDefinition m_oPageDef;
        public Int32 m_nChannelID;
        public SearchOrderByObject[] m_oOrderBy;
    }
}
