using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class ExtraRequestObject
    {
        public ExtraRequestObject() 
        {
            m_bNoCache = false;
            m_bZip = false;
            m_bWithInfo = false;
            m_bWithFileTypes = false;
            m_bUseFinalEndDate = false;
        }

        public void Initialize(bool bNoCache, bool bZip, bool bWithInfo, bool bWithFileTypes, bool bUseFinalEndDate)
        {
            m_bNoCache = bNoCache;
            m_bZip = bZip;
            m_bWithInfo = bWithInfo;
            m_bWithFileTypes = bWithFileTypes;
            m_bUseFinalEndDate = bUseFinalEndDate;
        }

        public bool m_bNoCache;
        public bool m_bZip;
        public bool m_bWithInfo;
        public bool m_bWithFileTypes;
        public bool m_bUseFinalEndDate;
    }
}
