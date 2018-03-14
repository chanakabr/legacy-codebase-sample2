using ConfigurationManager;
using System;

public partial class whoami : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string m_sServerName = ApplicationConfiguration.ServerName.Value;
        if (string.IsNullOrEmpty(m_sServerName))
        {
            m_sServerName = "Unknown";
        }

        string m_sApplicationName = ApplicationConfiguration.ApplicationName.Value;
        if (string.IsNullOrEmpty(m_sApplicationName))
        {
            m_sApplicationName = "Unknown";
        }



        Response.Write("Server: " + m_sServerName + "<br/>Application: " + m_sApplicationName);
    }
}
