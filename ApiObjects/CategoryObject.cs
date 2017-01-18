using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class CategoryObject
    {
        public CategoryObject() 
        {
            m_oCategories = null;
            m_oChannels = null;
            m_sTitle = "";
            m_nID = 0;
        }

        public void Initialize(CategoryObject[] oCategories , ChannelObject[] oChannels , string sTitle , Int32 nID)
        {
            m_oCategories = oCategories;
            m_oChannels = oChannels;
            m_sTitle = sTitle;
            m_nID = nID;
        }

        public CategoryObject[] m_oCategories;
        public ChannelObject[] m_oChannels;
        public string m_sTitle;
        public Int32 m_nID;
    }
}
