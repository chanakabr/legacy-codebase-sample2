using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.IO;
using System.Text;

public partial class GoogleInApp : System.Web.UI.Page
{

    protected override void OnInit(EventArgs e)
    {
      
    }

    
    protected void Page_Load(object sender, EventArgs e)
    {
        
     
        

        
        

    }
    public string theJWT(int customdateID)
    {
        try
        {
            return ConditionalAccess.Utils.GetGoogleSignature(134, customdateID);
           
        }
        catch
        {
            return string.Empty;
        }

    }
}