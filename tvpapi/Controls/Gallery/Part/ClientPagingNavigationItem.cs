using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Tvinci.Web.Controls.Gallery.Part
{
    public class ClientPagingNavigationItem : Control, INamingContainer
    {
        public string GalleryID { get; set; }
        public int ButtonNumber { get; set; }
    }
}
