using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class PlayListSchema
    {
        public PlayListSchema() 
        {
            m_oBreak = null;
            m_oOverlay = null;
            m_oPost = null;
            m_oPre = null;
            m_nChannelID = 0;
        }

        public void Initialize(Int32 nChannelID, MediaAdObject oPre , MediaAdObject oPost , MediaAdObject oBreak , MediaAdObject oOverlay)
        {
            m_nChannelID = nChannelID;
            m_oBreak = oBreak;
            m_oOverlay = oOverlay;
            m_oPost = oPost;
            m_oPre = oPre;
        }

        public Int32 m_nChannelID;
        public MediaAdObject m_oPre;
        public MediaAdObject m_oPost;
        public MediaAdObject m_oBreak;
        public MediaAdObject m_oOverlay;
    }
}
