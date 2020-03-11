using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Tvinci.Web.Controls.Gallery
{
    
    public interface IGalleryPart
    {
        void HandleAddedToGallery(GalleryBase gallery);         
        string HandlerID { get; }            
    }        
}
