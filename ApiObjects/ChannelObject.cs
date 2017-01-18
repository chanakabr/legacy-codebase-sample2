using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class ChannelObject
    {
        public ChannelObject() 
        {
            m_oPlayListSchema = null;
            m_oMediaObjects = null;
            m_nChannelTotalSize = 0;
            m_oPicObjects = null;
            m_sTitle = "";
            m_sDescription = "";
            m_nID = 0;
            m_sEditorRemarks = "";
        }

        public void Initialize(PlayListSchema oPlayListSchema, MediaObject[] oMediaObjects, Int32 nChannelTotalSize, PicObject[] oPicObjects , string sTitle , string sDescription , string sEditorRemarks, Int32 nID)
        {
            m_oPlayListSchema = oPlayListSchema;
            m_oMediaObjects = oMediaObjects;
            m_nChannelTotalSize = nChannelTotalSize;
            m_oPicObjects = oPicObjects;
            m_sTitle = sTitle;
            m_sDescription = sDescription;
            m_sEditorRemarks = sEditorRemarks;
            m_nID = nID;
        }

        public PlayListSchema m_oPlayListSchema;
        public MediaObject[] m_oMediaObjects;
        public Int32 m_nChannelTotalSize;
        public PicObject[] m_oPicObjects;
        public string m_sTitle;
        public string m_sDescription;
        public string m_sEditorRemarks;
        public Int32 m_nID;
    }
}
