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
using Dundas.Olap.Data.AdomdNet;
using Dundas.Olap.Data;

namespace StatisticsService
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Write("ASP User: " + System.Security.Principal.WindowsIdentity.GetCurrent().Name);
            Response.Write("<br/>");

            try
            {
                Page p = new Page();

                AdomdNetDataProvider prov = new AdomdNetDataProvider();
                p.Controls.Add(prov);

                prov.CacheType = SchemaCacheType.None;
                prov.ConnectionString = "Data Source=msd101.1host.co.il; Provider=msolap;Catalog=TvinciMedia;";
                prov.Open();

                Response.Write("Connected");
            }
            catch (Exception ex)
            {
                Response.Write("Failed connecting. Error:" + ex.Message);
            }
        }
    }
}
