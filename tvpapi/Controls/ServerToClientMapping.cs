using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Tvinci.Web.Controls
{    
    public class ServerToClientMapping : WebControl
    {
        public String ControlName { get; set; }


        public ServerToClientMapping()
        {
            
        }
        protected override void OnPreRender(EventArgs e)
        {
            string code = string.Format("var {1} = '{0}{1}';", this.ClientID.Substring(0, this.ClientID.Length - this.ID.Length), ControlName);
            ScriptManager.RegisterClientScriptBlock(Page, typeof(ServerToClientMapping), this.ClientID, code, true);

            base.OnPreRender(e);
        }
        protected override void Render(System.Web.UI.HtmlTextWriter writer)
        {
            // no implementation by design
            
        }
        
    }
}

