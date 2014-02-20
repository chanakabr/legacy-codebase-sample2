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
        string m_sServerName = "";
        if (ConfigurationManager.AppSettings["SERVER_NAME"] != null &&
            ConfigurationManager.AppSettings["SERVER_NAME"].ToString() != "")
            m_sServerName = ConfigurationManager.AppSettings["SERVER_NAME"].ToString();
        else
            m_sServerName = "Unknown";

        string m_sApplicationName = "";
        if (ConfigurationManager.AppSettings["APPLICATION_NAME"] != null &&
            ConfigurationManager.AppSettings["APPLICATION_NAME"].ToString() != "")
            m_sApplicationName = ConfigurationManager.AppSettings["APPLICATION_NAME"].ToString();
        else
            m_sApplicationName = "Unknown";

        Response.Write("Server: " + m_sServerName + "<br/>Application: " + m_sApplicationName);
    }
}
