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
    private delegate object actionFunction(params object[] prms);
    
    public void ProcessRequest(HttpContext context) {
        GWFrontController fc = new GWFrontController("macdummy", TVPApi.PlatformType.STB);
        actionFunction actionFunc = null;
        List<object> paramsToFunc = new List<object>();
        
        //XXX
        switch (context.Request["type"])
        {
            case null:
                actionFunc = fc.GetServiceURLs;
                break;
            case "channel":
                actionFunc = fc.GetAllChannels;
                paramsToFunc.Add(long.Parse(context.Request["chid"]));
                break;
            case "category":
                actionFunc = fc.GetChannelMedias;
                paramsToFunc.Add(long.Parse(context.Request["intChid"]));
                break;
            case "content":
                string titId = context.Request["titId"];
                string sMediaID = titId.Split('-')[0];
                string channelId = titId.Split('-')[2];
                string sMediaType = titId.Contains("-") ? titId.Split('-')[1] : "272";
                
                actionFunc = fc.GetMediaInfo;
                paramsToFunc.Add(long.Parse(sMediaID));
                paramsToFunc.Add(int.Parse(sMediaType));
                break;
            default:
                break;
        }

        //XXX
        if (actionFunc == null)
            return;

        object resObj = actionFunc(paramsToFunc.ToArray());
        Type resType = resObj.GetType();
        System.Xml.Serialization.XmlSerializer xSerializer = new System.Xml.Serialization.XmlSerializer(resType);
        string serializedXML = null;
        using (StringWriter stringWriter = new StringWriter())
        {
            xSerializer.Serialize(stringWriter, resObj);
            serializedXML = stringWriter.ToString();
        }                
        string xslt = getXSLTByDeviceName("netgem");

        // Transforming the XML to appropriate device response
        XslCompiledTransform transform = new XslCompiledTransform();
        transform.Load(new XmlTextReader(xslt, XmlNodeType.Document, null));
        XPathDocument xpd = new XPathDocument(new StringReader(serializedXML));
        using (StringWriter sr = new StringWriter())
        {                        
            //Due to problems changing the encoding of the resulted XML, we prepend it manually
            transform.Transform(xpd.CreateNavigator(), getXSLTArgsList(), sr);            
            context.Response.Write(sr.ToString().Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"));
        }                
    }

    private XsltArgumentList getXSLTArgsList()
    {
        XsltArgumentList xslArg = new XsltArgumentList();
        string externalChannel = "201";
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
            return true;
        }
    }

}