using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVPApi;
using System.Text;
using System.Web.Script.Serialization;

public partial class Gateways_TestGateway : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        int groupID = int.Parse(Request.QueryString["GroupID"]);
        PlatformType platform = (PlatformType)Enum.Parse(typeof(PlatformType), Request.QueryString["Platform"]);
        SiteMapManager mngr = SiteMapManager.GetInstance;
        if (mngr != null)
        {
            //Dictionary<string, SiteMapManager> mngrDict = mngr.GetInstances();
            //if (mngrDict != null)
            //{
            //    foreach (KeyValuePair<string , SiteMapManager>
            //}
        }
    }

    public class ResponseObj
    {
        public string KeyStr;
        //string siteMap;
    }

    private string CreateJson(object obj)
    {
        StringBuilder sb = new StringBuilder();
        JavaScriptSerializer jsSer = new JavaScriptSerializer();
        jsSer.Serialize(obj, sb);
        return sb.ToString();
    }
}
