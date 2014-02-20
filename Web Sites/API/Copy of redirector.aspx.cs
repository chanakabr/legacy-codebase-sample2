using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.IO;
using System.Net;
using System.Text;

public partial class redirector : System.Web.UI.Page
{

    protected string GetFormParameters()
    {
        Int32 nCount = Request.TotalBytes;
        string sFormParameters = System.Text.Encoding.UTF8.GetString(Request.BinaryRead(nCount));
        return sFormParameters;
    }

    static public string SendXMLHttpReq(string sUrl, string sToSend , string sSoapHeader)
    {
        //Create the HTTP POST request and the authentication headers
        HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(sUrl));
        oWebRequest.Method = "post";
        oWebRequest.ContentType = "text/xml; charset=utf-8";
        oWebRequest.Headers["SOAPAction"] = sSoapHeader;
        
        byte[] encodedBytes = Encoding.UTF8.GetBytes(sToSend);
        oWebRequest.ContentLength = encodedBytes.Length;
        oWebRequest.AllowWriteStreamBuffering = true;

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
        catch(Exception ex)
        {
            return ex.Message + "   " + sUrl + "   " + sSoapHeader + "   " + HttpContext.Current.Request.Headers["SOAPAction"] + "   " +  sToSend;
        }
    }

    protected string GetUrl(string sNameSpace , string sFunction , ref string sUN , ref string sPass)
    {
        //Each function should have a user name and a password.
        //The URL of the web service is determined by the namespace

        Uri u = new Uri(sNameSpace);
        if (u.Host == "pricing.tvinci.com")
        {
            sUN = "pricing";
            sPass = "11111";
            return "https://platform.tvinci.com/pricing/module.asmx";
        }

        if (u.Host == "users.tvinci.com")
        {
            sUN = "pricing";
            sPass = "11111";
            return "https://platform.tvinci.com/users/module.asmx";
        }


        if (u.Host == "ca.tvinci.com")
        {
            sUN = "conditionalaccess";
            sPass = "11111";
            return "https://platform.tvinci.com/ca/module.asmx";
        }

        if (u.Host == "api.tvinci.com")
        {
            sUN = "api";
            sPass = "11111";
            return "https://platform.tvinci.com/api/api.asmx";
        }

        if (u.Host == "billing.tvinci.com")
        {
            sUN = "billing";
            sPass = "11111";
            return "https://platform.tvinci.com/billing/module.asmx";
        }

        return "";
    }

    static public string ConvertXMLToString(ref XmlDocument theDoc)
    {
        System.IO.StringWriter sw = new System.IO.StringWriter();
        XmlTextWriter xw = new XmlTextWriter(sw);
        theDoc.WriteTo(xw);
        return sw.ToString();
    }

    protected bool CheckIfUserOK(string sSiteGUID)
    {
        //here we shoul check if the user is equal to the user which is loged in
        return true;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string sRequest = GetFormParameters();
        XmlDocument theDoc = new XmlDocument();
        theDoc.LoadXml(sRequest);
        
        Int32 nCount = theDoc.GetElementsByTagName("sUserGUID").Count;
        bool bUserOK = true;
        for (int i = 0; i < nCount; i++)
        {
            string sSiteGUID = "";
            if (theDoc.GetElementsByTagName("sUserGUID")[i].FirstChild != null)
                sSiteGUID = theDoc.GetElementsByTagName("sUserGUID")[i].FirstChild.Value;
            if (sSiteGUID != "")
                bUserOK = CheckIfUserOK(sSiteGUID);
            if (bUserOK == false)
                return;
        }

        string sNameSpace = theDoc.GetElementsByTagName("Body" , "*")[0].FirstChild.NamespaceURI.ToLower().Trim();
        string sFunction = theDoc.GetElementsByTagName("Body" , "*")[0].FirstChild.LocalName;
        
        string sUN = "";
        string sPass = "";

        string sUrl = GetUrl(sNameSpace , sFunction , ref sUN , ref sPass);
        if (theDoc.GetElementsByTagName("sWSUserName", "*")[0].FirstChild != null)
        {
            theDoc.GetElementsByTagName("sWSUserName", "*")[0].FirstChild.Value = sUN;
        }
        else
        {
            XmlText t = theDoc.CreateTextNode(sUN);
            theDoc.GetElementsByTagName("sWSUserName", "*")[0].AppendChild(t);
        }
        if (theDoc.GetElementsByTagName("sWSPassword", "*")[0].FirstChild != null)
            theDoc.GetElementsByTagName("sWSPassword", "*")[0].FirstChild.Value = sPass;
        else
        {
            XmlText t = theDoc.CreateTextNode(sPass);
            theDoc.GetElementsByTagName("sWSPassword", "*")[0].AppendChild(t);
        }
        sRequest = ConvertXMLToString(ref theDoc);
        string sResp = SendXMLHttpReq(sUrl, "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + sRequest, sNameSpace + sFunction);
        Response.ClearHeaders();
        Response.Clear();
        Response.ContentType = "text/xml; charset=utf-8";
        Response.Expires = -1;
        
        //Response.Write(sRequest);
        Response.Write(sResp);
        
    }
}
