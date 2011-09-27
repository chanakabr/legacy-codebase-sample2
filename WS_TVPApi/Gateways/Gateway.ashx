<%@ WebHandler Language="C#" Class="Gateway" %>

using System;
using System.Web;
using System.Xml.Xsl;
using System.Xml;
using System.IO;
using System.Xml.XPath;
using System.Collections.Generic;

public class Gateway : IHttpHandler
{
    private delegate string actionFunction(params object[] prms);
    
    public void ProcessRequest(HttpContext context) {
        GWFrontController fc = new GWFrontController("macdummy", TVPApi.PlatformType.STB);
        actionFunction actionFunc = null;
        List<object> paramsToFunc = new List<object>();
        
        //XXX
        switch (context.Request["type"])
        {
            case "channel":
                actionFunc = fc.GetAllChannels;
                paramsToFunc.Add(long.Parse(context.Request["chid"]));
                break;
            case "category":
                actionFunc = fc.GetChannelMedias;
                paramsToFunc.Add(long.Parse(context.Request["intChid"]));
                break;
            default:
                break;
        }

        //XXX
        if (actionFunc == null)
            return;

        string resXML = actionFunc(paramsToFunc.ToArray());
        
        XslCompiledTransform transform = new XslCompiledTransform();        
        string xslt = getXSLTByDeviceName("netgem");        
        transform.Load(new XmlTextReader(xslt, XmlNodeType.Document, null));
        
        XPathDocument xpd = new XPathDocument(new StringReader(resXML));
        using (StringWriter sr = new StringWriter())
        {
            transform.Transform(xpd.CreateNavigator(), getXSLTArgsList(), sr);
            context.Response.Write(sr.ToString());
        }                
    }

    private XsltArgumentList getXSLTArgsList()
    {
        XsltArgumentList xslArg = new XsltArgumentList();
        string externalChannel = "50";
        xslArg.AddParam("chid", string.Empty, externalChannel);

        return xslArg;
    }

    private string getXSLTByDeviceName(string devName)
    {
        return File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Transformations/" + devName + ".xslt");
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}