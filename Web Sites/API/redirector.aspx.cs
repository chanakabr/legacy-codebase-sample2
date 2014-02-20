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
using System.Xml.XPath;
using System.Xml.Xsl;

/// <summary>
/// Transformer performs an XSLT Transformation.
/// It can be re-used, and is (probably) thread-safe.
/// </summary>
public class XsltTransformer 
{
    private XmlDocument m_objXMLDoc;
    private XslTransform m_objXSLTransform;
    public XsltTransformer() 
    {
        m_objXMLDoc =new XmlDocument();
        m_objXSLTransform = new XslTransform();
    }
    // With an XmlDocument
    public void setXML(XmlDocument input) 
    {
        m_objXMLDoc = input;
    }
    // With an input stream
    public void setXML(Stream input) 
    {
        if(input!=null)
            m_objXMLDoc.Load(input);
    }
    // With an XML reader
    public void setXML(XmlReader reader) 
    {
        if(reader!=null)
            m_objXMLDoc.Load(reader);
    }
    // With a plain string
    public void setXML(string strXML) 
    {
        if(strXML.Length > 0)
            m_objXMLDoc.LoadXml(strXML);
    }
    public string getXML() 
    {
        if(m_objXMLDoc!=null)
            return m_objXMLDoc.InnerXml;
        else
            return null;
    }

    // With a plain stringpath
    public void setXSL(string strXMLPath) 
    {
        XmlDocument d = new XmlDocument();
        d.LoadXml(strXMLPath);
        if(strXMLPath.Length > 0)
            m_objXSLTransform.Load(d);
    }
    // With an XmlDocument instance
    public void setXSL(XmlDocument xsl) 
    {
        m_objXSLTransform.Load(xsl.DocumentElement.CreateNavigator());
    }

    public void Transform(System.IO.TextWriter output) 
    {
        XPathNavigator navigator = m_objXMLDoc.DocumentElement.CreateNavigator();
        XmlTextWriter writer = new XmlTextWriter(output);
        m_objXSLTransform.Transform(navigator, null, writer);
    }
    public string TransformToString() 
    {
        XPathNavigator navigator = m_objXMLDoc.DocumentElement.CreateNavigator();
        StringBuilder sb = new StringBuilder();
        StringWriter swriter = new StringWriter(sb);
        XmlTextWriter writer = new XmlTextWriter(swriter);
        m_objXSLTransform.Transform(navigator, null, writer);
        return sb.ToString();
    }
}


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

    static public string ConvertXMLToString(ref XmlNode theDoc)
    {
        if (theDoc == null)
            return "";
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

    protected string GetXsl()
    {
        string sRet = "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">  <xsl:output method=\"xml\" indent=\"no\" encoding=\"UTF-8\"/>  <xsl:template match=\"/|comment()|processing-instruction()\">    <xsl:copy>      <xsl:apply-templates/>    </xsl:copy>  </xsl:template>  <xsl:template match=\"*\">    <xsl:element name=\"{local-name()}\">      <xsl:apply-templates select=\"@*|node()\"/>    </xsl:element>  </xsl:template>  <xsl:template match=\"@*\">    <xsl:attribute name=\"{local-name()}\">      <xsl:value-of select=\".\"/>    </xsl:attribute>  </xsl:template></xsl:stylesheet>";
        return sRet;
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
        XsltTransformer trans = new XsltTransformer();
        trans.setXML(theDoc);
        trans.setXSL(GetXsl());



        sRequest = trans.TransformToString();
        XmlDocument d = new XmlDocument();
        
        d.LoadXml(sRequest);

        XmlAttribute xmlns = d.CreateAttribute("xmlns");
        xmlns.Value = sNameSpace;
        d.GetElementsByTagName(sFunction)[0].Attributes.Append(xmlns);

        XmlNode xmlFunc = (XmlNode)(d.GetElementsByTagName("Body", "*")[0].FirstChild);
        sRequest = "<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body>";
        sRequest += ConvertXMLToString(ref xmlFunc);
        sRequest += "</soap:Body></soap:Envelope>";
        //sRequest = ConvertXMLToString(ref theDoc);
        string sResp = SendXMLHttpReq(sUrl, sRequest, "\"" + sNameSpace + sFunction + "\"");
        Response.ClearHeaders();
        Response.Clear();
        Response.ContentType = "text/xml; charset=utf-8";
        Response.Expires = -1;
        
        //Response.Write(sRequest);
        Response.Write(sResp);
        
    }
}
