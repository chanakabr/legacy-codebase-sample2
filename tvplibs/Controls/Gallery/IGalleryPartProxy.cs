using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

namespace Tvinci.Web.Controls.Gallery
{
    public interface IGalleryPartProxy
    {
        void NotifyGalleryOnCommandChange(CommandEventArgs e);
    }
}
