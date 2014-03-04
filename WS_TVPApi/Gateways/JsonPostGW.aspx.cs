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
using Logger;
using System.Xml;

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

        Stream body = Request.InputStream;
        Encoding encoding = Request.ContentEncoding;
        StreamReader reader = new System.IO.StreamReader(body, encoding);

        string sJsonRequest = reader.ReadToEnd();


        if (!string.IsNullOrEmpty(sJsonRequest))
        {
            JObject json = JObject.Parse(sJsonRequest);

            foreach (KeyValuePair<string, JToken> pair in json)
            {
                string sValue = string.Empty;

                if (pair.Value.GetType() == typeof(JArray))
                    sValue = pair.Value.ToString(Newtonsoft.Json.Formatting.None);
                else
                    sValue = (!pair.Key.Equals("initObj") &&
                              !pair.Key.Equals("tagPairs") &&
                              !pair.Key.Equals("metaPairs") &&
                              !pair.Key.Equals("userBasicData") &&
                              !pair.Key.Equals("userDynamicData")) &&
                              !pair.Key.Equals("orderObj") ? pair.Value.ToString(Newtonsoft.Json.Formatting.None).Replace("\"", @"") : pair.Value.ToString(Newtonsoft.Json.Formatting.None);

                HttpContext.Current.Items.Add(pair.Key, sValue);
            }
        }

        // add web service
        MethodFinder queryServices = new MethodFinder(m_MediaService,
                                                        m_SiteService,
                                                        m_PricingService,
                                                        m_DomainService,
                                                        m_BillingService,
                                                        m_ConditionalAccessService,
                                                        m_SocialService,
                                                        m_UsersService,
                                                        m_NotificationService);

        queryServices.ProcessRequest(sJsonRequest);                     
    }
}