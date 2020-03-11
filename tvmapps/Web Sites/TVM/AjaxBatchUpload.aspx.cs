using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;

public partial class AjaxBatchUpload : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        
        for (int i = 0; i < 10000000; i++)
        {
            Debug.WriteLine(i+" Hello");
        }

        string sFileUrl = Request.QueryString["u"];
    }
}