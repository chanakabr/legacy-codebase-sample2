using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using Tvinci.Web.Controls.Gallery.Part;
using System.Web.UI.WebControls;

namespace Tvinci.Web.Controls.Gallery
{
    public class PartWrapper : PlaceHolder
    { }


    public abstract class GalleryPartHandler : IGalleryPartHandler
    {
        public abstract string GetIdentifier();
        
        #region IGalleryPartHandler Members

        string IGalleryPartHandler.Identifier
        {
            get
            {
                return GetIdentifier();
            }
        }
        
        #endregion

        protected Control FindPartWrapper(GalleryPart part, Control defaultControl)
        {
            if (part.IsPartWrapped)
            {
                Control parent = part.Parent;

                while (parent != null && !(parent is GalleryControl))
                {
                    if (parent is PartWrapper)
                    {
                        return parent;
                    }

                    parent = parent.Parent;
                }
            }

            return defaultControl;
        }
    }
}
