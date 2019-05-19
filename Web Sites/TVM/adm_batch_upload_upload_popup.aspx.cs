using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using TVinciShared;
using System.Data;
using System.Threading;
using System.ComponentModel;
using System.Runtime.Remoting.Messaging;
using System.Net;

public partial class adm_batch_upload_upload_popup : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void UploadExcel(object sender, EventArgs e)
    {
        Page.ClientScript.RegisterStartupScript(this.GetType(), "alert", "batchupload('ggg');", true);
    }
    
}