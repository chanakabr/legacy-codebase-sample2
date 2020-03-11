using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class FileRequestObject
    {
        public FileRequestObject() 
        {
            m_sFileFormat = "";
            m_sFileQuality = "";
        }

        public void Initialize(string sFileFormat , string sFileQuality)
        {
            m_sFileFormat = sFileFormat;
            m_sFileQuality = sFileQuality;
        }

        public string m_sFileFormat;
        public string m_sFileQuality;
    }
}
