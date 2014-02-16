using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MeidaMaper
    {
        public MeidaMaper()
        {
            m_nMediaFileID = 0;
            m_nMediaID = 0;
        }
        public void Initialize(Int32 nMediaFileID, Int32 nMediaID)
        {
            m_nMediaFileID = nMediaFileID;
            m_nMediaID = nMediaID;
        }

        public Int32 m_nMediaFileID;
        public Int32 m_nMediaID;
    }
}
