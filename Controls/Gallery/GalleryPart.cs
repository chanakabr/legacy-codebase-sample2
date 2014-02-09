using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.ComponentModel;
using Tvinci.Web.Controls.ContainerControl;

namespace Tvinci.Web.Controls.Gallery
{
    public abstract class GalleryPart : XHtmlContainer, IGalleryPart
    {
        bool m_isPartWrapped = false;
        public bool IsPartWrapped { get { return m_isPartWrapped; } set { m_isPartWrapped = value; } }

        #region Constructor
        public GalleryPart()            
        {
            // by default prevent viewstate in panels
            base.EnableViewState = false;            
        }                   
        #endregion

        

        
        

        //public virtual void PostProcess()
        //{            
        //}

        //public virtual void PreProcess()
        //{         
        //}
        
        #region IGalleryPart3 Members
        public abstract string HandlerID{get;}        
        #endregion        
          
        #region IGalleryPart Members

        public virtual void HandleAddedToGallery(GalleryBase gallery)
        {
            
        }

        #endregion
    }  


    
}
