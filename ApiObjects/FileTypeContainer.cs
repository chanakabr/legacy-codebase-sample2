using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class FileTypeContainer
    {
        public FileTypeContainer()
        {
            m_sType = "";
            m_nFileTypeID = 0;
        }
        public void Initialize(string sType, Int32 nID)
        {
            m_sType = sType;
            m_nFileTypeID = nID;
        }

        public Int32 m_nFileTypeID;
        public string m_sType;
    }
}
