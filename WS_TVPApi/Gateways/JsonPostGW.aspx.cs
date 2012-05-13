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
        Response.ContentType = "application/json; charset=utf-8";

        System.IO.Stream body = Request.InputStream;
        System.Text.Encoding encoding = Request.ContentEncoding;
        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

        string sJsonRequest = reader.ReadToEnd();

        if (!string.IsNullOrEmpty(sJsonRequest))
        {
            Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(sJsonRequest);

            foreach (KeyValuePair<string, Newtonsoft.Json.Linq.JToken> pair in json)
            {
                string sValue = (!pair.Key.Equals("initObj")) ? pair.Value.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", @"") : pair.Value.ToString(Newtonsoft.Json.Formatting.None);
                HttpContext.Current.Items.Add(pair.Key, sValue);
            }
        }

        MethodFinder queryServices = new MethodFinder(m_MediaService, m_SiteService);

        queryServices.ProcessRequest();
    }
}