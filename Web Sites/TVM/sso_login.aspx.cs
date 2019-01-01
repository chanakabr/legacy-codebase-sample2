using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using TVinciShared;

public partial class clear_cache : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string remoteHost = "";
        string httpRefferer = "";
        if (HttpContext.Current.Request.ServerVariables["REMOTE_HOST"] != null)
            remoteHost = HttpContext.Current.Request.ServerVariables["REMOTE_HOST"].ToLower();
        if (HttpContext.Current.Request.ServerVariables["HTTP_REFERER"] != null)
            httpRefferer = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"].ToLower();

        if (Request.QueryString["ks"] != null)
        {
            string ks = Request.QueryString["ks"];
            string username;
            int partnerId;
            GetUsernameAndPartnerId(ks, out username, out partnerId);

            if (!string.IsNullOrEmpty(username))
            {
                string errorMessage = string.Empty;

                HttpContext.Current.Session["LoginGroup"] = partnerId;
                bool loginResult = LoginManager.LoginToSite(username, string.Empty, ref errorMessage, true);

                HttpContext.Current.Session["LoginGroup"] = partnerId;

                if (loginResult)
                {
                    Response.Redirect("adm_ppv_modules.aspx");
                }
            }
        }
        else
            Response.StatusCode = 404;
    }

    private static void GetUsernameAndPartnerId(string ks, out string username, out int partnerId)
    {
        username = string.Empty;
        partnerId = 0;
        username = GetUsername(ks);
        partnerId = GetPartnerId(ks);
    }

    private static string GetUsername(string ks)
    {
        string username = string.Empty;
        JObject jsonBody = new JObject();
        jsonBody["ks"] = ks;

        JObject jsonResult = CallRest("ottuser", "get", jsonBody);
        var resultObjects = jsonResult["result"]["objects"] as JArray;
        username = (resultObjects[0] as JObject)["username"].ToString();

        return username;
    }

    private static int GetPartnerId(string ks)
    {
        JObject jsonBody = new JObject();
        jsonBody["ks"] = ks;
        jsonBody["session"] = ks;

        JObject jsonResult = CallRest("session", "get", jsonBody);

        int partnerId = Convert.ToInt32(jsonResult["result"]["partnerId"]);

        return partnerId;
    }

    private static JObject CallRest(string service, string action, JObject body)
    {
        JObject result = null;

        string baseUrl = System.Configuration.ConfigurationManager.AppSettings["HackathonRest"];

        string requestUrl = string.Format("{0}/service/{1}/action/{2}", baseUrl, service, action);

        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
        webRequest.ContentType = "application/json";
        webRequest.Method = "POST";

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(body.ToString());
        webRequest.ContentLength = bytes.Length;
        System.IO.Stream os = webRequest.GetRequestStream();
        os.Write(bytes, 0, bytes.Length);
        os.Close();

        try
        {
            string res = string.Empty;
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            HttpStatusCode sCode = webResponse.StatusCode;
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(webResponse.GetResponseStream());
                res = sr.ReadToEnd();

                result = JObject.Parse(res);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }

        }
        catch (WebException ex)
        {
            StreamReader errorStream = null;
            try
            {
                errorStream = new StreamReader(ex.Response.GetResponseStream());
                string res = errorStream.ReadToEnd();
            }
            finally
            {
                if (errorStream != null) errorStream.Close();
            }
        }

        return result;
    }
}
