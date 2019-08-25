using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

namespace Tvinci.Web.Controls.Gallery.Part
{
    public class TabPartItem : Control, INamingContainer
    {
        public bool IsActive { get; set; }
        public int Index { get; set; }
        public string Title { get; set; }
        public long? ItemsCount { get; set; }
        
        public TabPartItem(int index, string title, bool isActive)
        {
            IsActive = isActive;
            Index = index;
            Title = title;            
        }
    }
}
