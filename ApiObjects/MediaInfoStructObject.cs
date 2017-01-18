using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MediaInfoStructObject
    {
        public MediaInfoStructObject() 
        {
            m_sMetaStrings = null;
            m_sMetaDoubles = null;
            m_sMetaBools = null;
            m_sTags = null;
            m_bTitle = false;
            m_bDescription = false;
            m_bStatistics = false;
            m_bPersonal = false;
            m_bType = false;
        }

        public void Initialize(string[] sMetaStrings , string[] sMetaDoubles , string[] sMetaBools, string[] sTags , bool bTitle , bool bDescription , bool bStatistics , bool bPersonal , bool bType)
        {
            m_sMetaStrings = sMetaStrings;
            m_sMetaBools = sMetaBools;
            m_sMetaDoubles = sMetaDoubles;
            m_sTags = sTags;
            m_bTitle = bTitle;
            m_bDescription = bDescription;
            m_bStatistics = bStatistics;
            m_bPersonal = bPersonal;
            m_bType = bType;
        }

        public bool m_bTitle;
        public bool m_bDescription;
        public bool m_bStatistics;
        public bool m_bPersonal;
        public bool m_bType;
        public string[] m_sMetaStrings;
        public string[] m_sMetaDoubles;
        public string[] m_sMetaBools;
        public string[] m_sTags;
    }
}
