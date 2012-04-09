using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVPApi;

public partial class Gateways_JsonPostGW : BaseGateway
{     
    protected void Page_Load(object sender, EventArgs e)
    {
        MethodFinder queryServices = new MethodFinder(m_MediaService, m_SiteService);
        queryServices.ProcessRequest();
    }
   
}