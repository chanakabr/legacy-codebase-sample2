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

    static public string SendXMLHttpReq(string sUrl, string sToSend)
    {
        //Create the HTTP POST request and the authentication headers
        HttpWebRequest oWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(sUrl));
        oWebRequest.Method = "post";
        oWebRequest.ContentType = "text/xml";
        oWebRequest.Headers = (WebHeaderCollection)(HttpContext.Current.Request.Headers);
        byte[] encodedBytes = Encoding.UTF8.GetBytes(sToSend);
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
        catch
        {
            return "";
        }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        string sRequest = GetFormParameters();
        XmlDocument theDoc = new XmlDocument();
        theDoc.LoadXml(sRequest);
        Response.Write(sRequest);
        
    }
}
