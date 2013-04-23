using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVPApi;

public partial class Gateways_JsonPostGW : BaseGateway
{
    protected override void OnLoad(EventArgs e)
    {

        base.OnLoad(e);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "application/json; charset=utf-8";
        Response.AppendHeader("Access-Control-Allow-Origin", "*");

        System.IO.Stream body = Request.InputStream;
        System.Text.Encoding encoding = Request.ContentEncoding;
        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

        //List<TVPApi.TagMetaPair> tagPairs = new List<TagMetaPair>();
        //tagPairs.Add(new TagMetaPair("Genre", "Cas"));
        //tagPairs.Add(new TagMetaPair("Genre", "Cas"));

        //Object o = Newtonsoft.Json.Linq.JArray.FromObject(tagPairs);

        string sJsonRequest = reader.ReadToEnd();

        if (!string.IsNullOrEmpty(sJsonRequest))
        {
            Newtonsoft.Json.Linq.JObject json = Newtonsoft.Json.Linq.JObject.Parse(sJsonRequest);

            foreach (KeyValuePair<string, Newtonsoft.Json.Linq.JToken> pair in json)
            {
                string sValue = string.Empty;

                if (pair.Value.GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                {
                    sValue = pair.Value.ToString(Newtonsoft.Json.Formatting.None);
                }
                else
                {
                    sValue = (!pair.Key.Equals("initObj") && !pair.Key.Equals("tagPairs") && !pair.Key.Equals("metaPairs") && !pair.Key.Equals("userBasicData") && !pair.Key.Equals("userDynamicData")) ? pair.Value.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", @"") : pair.Value.ToString(Newtonsoft.Json.Formatting.None);
                }

                HttpContext.Current.Items.Add(pair.Key, sValue);
            }
        }

        MethodFinder queryServices = new MethodFinder(m_MediaService, m_SiteService, m_PricingService, m_DomainService, m_BillingService, m_ConditionalAccessService, m_SocialService, m_UsersService, m_NotificationService);

        queryServices.ProcessRequest();
    }
}