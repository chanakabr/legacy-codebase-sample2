using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Configuration;

public partial class whoami : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string m_sServerName = TVinciShared.WS_Utils.GetTcmConfigValue("SERVER_NAME");
        if (string.IsNullOrEmpty(m_sServerName))
        {
            m_sServerName = "Unknown";
        }

        string m_sApplicationName = TVinciShared.WS_Utils.GetTcmConfigValue("APPLICATION_NAME");
        if (string.IsNullOrEmpty(m_sApplicationName))
        {
            m_sApplicationName = "Unknown";
        }



        Response.Write("Server: " + m_sServerName + "<br/>Application: " + m_sApplicationName);
    }
}
