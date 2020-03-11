using Core.Catalog.Cache;
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

namespace WebAPI
{
    public partial class clear_cache : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string action = "";
            string key = "";
            if (Request.QueryString["action"] != null)
                action = Request.QueryString["action"].ToString().ToLower().Trim();
            if (Request.QueryString["key"] != null)
                key = Request.QueryString["key"].ToString().Trim();
            ApiObjects.Response.Status status = Core.Api.api.ClearLocalServerCache(action, key);
            Response.Clear();
            Response.Write(status.Message);
            Response.StatusCode = status.Code == 0 ? 200 : status.Code;
        }
    }
}