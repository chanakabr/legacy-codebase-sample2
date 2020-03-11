using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MediaAdSchema
    {
        public MediaAdSchema() 
        {
            m_oPre = null;
            m_oPost = null;
            m_oBreak = null;
            m_oOverlay = null;
            m_bPlaylistSchemaControlled = false;
        }

        public void Initialize(ApiObjects.MediaAdObject oPre , ApiObjects.MediaAdObject oPost , ApiObjects.MediaAdObject oBreak , ApiObjects.MediaAdObject oOverlay , bool bPlaylistSchemaControlled)
        {
            m_bPlaylistSchemaControlled = bPlaylistSchemaControlled;
            m_oPre = oPre;
            m_oPost = oPost;
            m_oBreak = oBreak;
            m_oOverlay = oOverlay;
        }

        public bool m_bPlaylistSchemaControlled;
        public ApiObjects.MediaAdObject m_oPre;
        public ApiObjects.MediaAdObject m_oPost;
        public ApiObjects.MediaAdObject m_oBreak;
        public ApiObjects.MediaAdObject m_oOverlay;
        
    }
}
