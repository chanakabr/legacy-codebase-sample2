using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class PicObject
    {
        public PicObject() 
        {
            m_nPicWidth = 0;
            m_nPicHeight = 0;
            m_sPicURL = "";
            m_nPicBrandHeight = 0;
            m_nPicBrandRecurringType = 0;
        }

        public void Initialize(Int32 nPicWidth , Int32 nPicHeight , FileRequestObject theFileReqObj , string sPicURL)
        {
            m_nPicWidth = nPicWidth;
            m_nPicHeight = nPicHeight;
            m_oFileRequestObj = theFileReqObj;
            m_sPicURL = sPicURL;
        }

        public Int32 m_nPicWidth;
        public Int32 m_nPicHeight;
        public FileRequestObject m_oFileRequestObj;
        public string m_sPicURL;
        public Int32 m_nPicBrandHeight;
        public Int32 m_nPicBrandRecurringType;
    }
}
