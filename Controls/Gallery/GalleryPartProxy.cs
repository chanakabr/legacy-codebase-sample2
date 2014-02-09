using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Tvinci.Web.Controls.Gallery
{    
    public class GalleryPartProxy : UserControl, IGalleryPartProxy
    {
        IGallery m_parent;
        bool m_registered = false;

        #region Override methods   
             
        protected override void OnInit(EventArgs e)
        {
            registerProxy(); // relevent when adding by code;                    
            base.OnInit(e);
        }
        protected override void OnLoad(EventArgs e)
        {
            registerProxy(); // relevent when using LoadControl;
            base.OnLoad(e);
        }
        #endregion

        #region Private methods                
        public void registerProxy()
        {
            if (m_registered)
            {
                return;
            }

            m_registered = true;

            Control tempParent = this.Parent;

            while (tempParent != null)
            {
                IGallery gallery = tempParent as IGallery;

                if (gallery != null)
                {
                    m_parent = gallery;
                    return;
                }
                else
                {
                    tempParent = tempParent.Parent;
                }
            }

            throw new Exception(string.Format("Control '{0}' of type 'GalleryPanelProxy' must be a child of control which implements 'IGallery' (Did you used a wrong gallery panel proxy or used it outside of gallery control?)", this.ID));
        }
        #endregion

        #region IGalleryPanelProxy Members

        public void NotifyGalleryOnCommandChange(CommandEventArgs e)
        {
            m_parent.RaiseCommandChangedInProxy(e);
        }

        #endregion
    }
}
