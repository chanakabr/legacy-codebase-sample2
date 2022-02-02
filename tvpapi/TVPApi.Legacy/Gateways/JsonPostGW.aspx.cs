using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVPApi;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Xml;
using KLogMonitor;
using System.Reflection;
using TVPPro.SiteManager.Helper;
using System.Security.Cryptography;
using System.Configuration;
using TVPApi.Common;
using Phx.Lib.Log;

public partial class Gateways_JsonPostGW : BaseGateway
{
    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        using (KMonitor km = new KMonitor(Phx.Lib.Log.Events.eEvent.EVENT_CLIENT_API_START, null, null, null, null))
        {
            Response.ContentType = "application/json; charset=utf-8";
            Response.AppendHeader("Access-Control-Allow-Origin", "*");

            Stream body = Request.InputStream;
            Encoding encoding = Request.ContentEncoding;
            StreamReader reader = new System.IO.StreamReader(body, encoding);

            string sJsonRequest = reader.ReadToEnd();

            var gateway = new JsonPostGateway();
            var response = gateway.ProcessRequest(sJsonRequest);
            HttpContext.Current.Response.HeaderEncoding = HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;
            HttpContext.Current.Response.Charset = "utf-8";
            HttpContext.Current.Response.Write(response);
        }
    }
}