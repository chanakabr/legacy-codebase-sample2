using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Net;
using System.Text;
using System.IO;

public partial class cloud_cache : System.Web.UI.Page
{

    static public string SendXMLHttpReq(string sUrl, string sToSend, string sSoapHeader)
    {
        //Create the HTTP POST request and the authentication headers
        HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(sUrl));
        oWebRequest.Method = "post";
        oWebRequest.ContentType = "text/xml; charset=utf-8";
        //oWebRequest.Headers["SOAPAction"] = sSoapHeader;

        byte[] encodedBytes = Encoding.UTF8.GetBytes(sToSend);
        //oWebRequest.ContentLength = encodedBytes.Length;
        //oWebRequest.AllowWriteStreamBuffering = true;

        //Send the request.
        Stream requestStream = oWebRequest.GetRequestStream();
        requestStream.Write(encodedBytes, 0, encodedBytes.Length);
        requestStream.Close();

        try
        {
            HttpWebResponse oWebResponse = (HttpWebResponse)oWebRequest.GetResponse();
            HttpStatusCode sCode = oWebResponse.StatusCode;
            Stream receiveStream = oWebResponse.GetResponseStream();

            StreamReader sr = new StreamReader(receiveStream);
            string resultString = sr.ReadToEnd();

            sr.Close();
            oWebRequest = null;
            oWebResponse = null;
            return resultString;
        }
        catch (WebException ex)
        {
            Console.WriteLine(ex);
            WebResponse errRsp = ex.Response;
            using (StreamReader rdr = new StreamReader(errRsp.GetResponseStream()))
            {
                return rdr.ReadToEnd();
            }
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string sGroupID = Request.QueryString["gid"];
        if (!string.IsNullOrEmpty(sGroupID))
        {
            if (sGroupID.Equals("109"))
            {
                Response.Write(SendXMLHttpReq("http://10.35.3.16/api_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.17/api_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.16/pricing_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.17/pricing_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.16/cas_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.17/cas_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.5/v1_1/clean_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.6/v1_1/clean_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.14/technicalsupport.aspx?clearcache=true", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.15/technicalsupport.aspx?clearcache=true", string.Empty, null));
                Response.Write("<br>");

            }
            if (sGroupID.Equals("147"))
            {
                Response.Write(SendXMLHttpReq("http://10.35.3.16/api_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.17/api_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.16/pricing_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.17/pricing_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.16/cas_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.17/cas_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.38.1.5:5000/v1_0/clean_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
               // Response.Write(SendXMLHttpReq("http://10.38.1.6:5000/v1_0/clean_cache.aspx?action=clear_all", string.Empty, null));
               // Response.Write("<br>");
                //Response.Write(SendXMLHttpReq("http://10.38.1.7:5000/v1_0/clean_cache.aspx?action=clear_all", string.Empty, null));
               // Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.12/v1_6_2/clearcache.aspx?clear=true", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.38.1.5/technicalsupport.aspx?clearcache=true", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.38.1.6/technicalsupport.aspx?clearcache=true", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.38.1.7/technicalsupport.aspx?clearcache=true", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.38.1.8/technicalsupport.aspx?clearcache=true", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://toggle.stg.tvinci.com/technicalsupport.aspx?clearcache=true", string.Empty, null));
                //bool retVal = TVinciShared.ProtocolsFuncs.BuildLucene("http://10.38.1.5:8090/Service.svc", 147);
                //if (retVal)
                //{
                //    Response.Write("Lucene 1 cleared");
                //    Response.Write("<br>");
                //}
                //retVal = TVinciShared.ProtocolsFuncs.BuildLucene("http://10.38.1.6:8090/Service.svc", 147);
                //if (retVal)
                //{
                //    Response.Write("Lucene 2 cleared");
                //    Response.Write("<br>");
                //}
                Response.Write("<br>");

            }
            if (sGroupID.Equals("134"))
            {
                Response.Write(SendXMLHttpReq("http://10.35.3.16/api_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.17/api_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.16/pricing_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.17/pricing_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.16/cas_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.17/cas_v1_2/clear_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.5/v1_1/clean_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://10.35.3.6/v1_1/clean_cache.aspx?action=clear_all", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://195.197.177.6/technicalsupport.aspx?clearcache=true", string.Empty, null));
                Response.Write("<br>");
                Response.Write(SendXMLHttpReq("http://195.197.177.5/technicalsupport.aspx?clearcache=true", string.Empty, null));
                Response.Write("<br>");

            }
        }
    }

    //protected void Page_Load(object sender, EventArgs e)
    //{

    //    string platformVersion = Request.QueryString["pv"];
    //    string sLocation = Request.QueryString["loc"];
    //    string wsVersion = Request.QueryString["wsv"];
    //    string sUSVersion = Request.QueryString["usv"];
    //    string tvpApiVer = Request.QueryString["tvpapiv"];
    //    string clientIPs = Request.QueryString["tvpapiv"];
    //    string groupID = Request.QueryString["gid"];
    //    string wsIP = ConfigurationManager.AppSettings["wsips"];
    //    string[] wsIPsArrs = wsIP.Split(';');
    //    string[] wsVersionArr = wsVersion.Split(';');
    //    foreach (string wsVersionStr in wsVersionArr)
    //    {
    //        foreach (string ip in wsIPsArrs)
    //        {
    //            string apiWsURL = string.Format("http://{0}/api_v{1}/clear_cache.aspx?action=clear_all", ip, wsVersionStr);
    //            SendXMLHttpReq(apiWsURL, string.Empty, null);
    //            string sCAUrl = string.Format("http://{0}/cas_v{1}/clear_cache.aspx?action=clear_all", ip, wsVersionStr);
    //            SendXMLHttpReq(sCAUrl, string.Empty, null);
    //            string sPricingUrl = string.Format("http://{0}/pricing_v{1}/clear_cache.aspx?action=clear_all", ip, wsVersionStr);
    //            SendXMLHttpReq(sPricingUrl, string.Empty, null);
    //            Response.Write("Web Services Cleared" + "</br>");
    //        }

    //    }

    //    if (!string.IsNullOrEmpty(sUSVersion))
    //    {
    //        string usPlatformIP = ConfigurationManager.AppSettings[string.Format("nyips")];
    //        string[] usPlatformIPArrs = usPlatformIP.Split(';');
    //        foreach (string ip in usPlatformIPArrs)
    //        {
    //            string platform = string.Format("http://{0}/{1}/clean_cache.aspx?action=clear_all", ip, sUSVersion);
    //            SendXMLHttpReq(platform, string.Empty, null);
    //            Response.Write("US Platform cleared" + "</br>");
    //        }
           
    //    }
      
    //    string platformIP = ConfigurationManager.AppSettings[string.Format("platform_{0}", sLocation)];
    //    string[] platformIPArrs = platformIP.Split(';');
    //    foreach (string ip in platformIPArrs)
    //    {
    //        string platform = string.Format("http://{0}/clean_cache.aspx?action=clear_all", ip);
    //        SendXMLHttpReq(platform, string.Empty, null);
    //        Response.Write("Platform cleared" + "</br>");
    //    }
       

    //    string tvpapiIP = ConfigurationManager.AppSettings["tvpapiips"];
    //    string[] tvpapiIPsArrs = tvpapiIP.Split(';');
    //    foreach (string ip in tvpapiIPsArrs)
    //    {
    //        string tvpapi = string.Format("http://{0}/v{1}/technicalsupport.aspx?clearcache=true", ip, tvpApiVer);
    //        //SendXMLHttpReq(tvpapi, string.Empty, null);
    //        Response.Write(SendXMLHttpReq(tvpapi, string.Empty, null) + "</br>" + "TVPApi cleared" + "</br>");
    //    }
        

    //    string clientIP = ConfigurationManager.AppSettings[string.Format("clientips_{0}",groupID)];
    //    string[] clientIPArrs = clientIP.Split(';');
    //    foreach (string ip in clientIPArrs)
    //    {
    //        string site = string.Format("http://{0}/technicalsupport.aspx?clearcache=true", ip);
    //        SendXMLHttpReq(site, string.Empty, null);
    //        Response.Write("Site cleared");
    //    }
        
       
    //}
}