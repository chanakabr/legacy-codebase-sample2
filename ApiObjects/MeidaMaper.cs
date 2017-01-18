using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MeidaMaper
    {
        public Int32 m_nMediaFileID;
        public Int32 m_nMediaID;
        public string m_sProductCode;

        public MeidaMaper()
        {
            m_nMediaFileID = 0;
            m_nMediaID = 0;
            m_sProductCode = string.Empty;
        }
        public void Initialize(Int32 nMediaFileID, Int32 nMediaID, string productCode)
        {
            this.m_nMediaFileID = nMediaFileID;
            this.m_nMediaID = nMediaID;
            this.m_sProductCode = productCode;
        }
    }
}
