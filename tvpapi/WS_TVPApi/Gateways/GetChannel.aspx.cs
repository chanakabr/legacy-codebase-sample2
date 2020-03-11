using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Gateways_GetChannel : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string sChannelID = Request.QueryString["ChannelID"].ToString();

        Server.Transfer(string.Format("~/gateways/jsongateway.aspx?UUID=test&ApiUser=tvpapi_125&ApiPass=11111&MethodName=GetChannelMediaList&ChannelID={0}&pageSize=20&pageIndex=0&picSize=224X124", sChannelID));
    }
}