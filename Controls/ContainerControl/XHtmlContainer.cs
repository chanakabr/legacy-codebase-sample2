using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Tvinci.Web.Controls.ContainerControl
{
    public class XHtmlContainer : WebControl, INamingContainer
    {
        protected override void Render(HtmlTextWriter writer)
        {
            this.RenderContents(writer);
        }
    }
}
