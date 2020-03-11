<%@ WebHandler Language="C#" Class="Gateway" %>

using System;
using System.Web;
using System.Xml.Xsl;
using System.Xml;
using System.IO;
using System.Xml.XPath;
using System.Collections.Generic;

public class Gateway : IHttpHandler, System.Web.SessionState.IRequiresSessionState, System.Web.SessionState.IReadOnlySessionState
{
    private delegate object actionFunction(params object[] prms);

    private long tvmChid;
    private long devChid;
    private GWFrontController.ApiAccessInfo info;

    public void ProcessRequest(HttpContext context)
    {
        string encodedQS = HttpUtility.UrlDecode(context.Request.QueryString.ToString());
        Dictionary<string, string> queryArgs = parseQS(encodedQS);

        string devSchemaQS = context.Request["devtype"];
        string devType = System.Configuration.ConfigurationManager.AppSettings[devSchemaQS];
        MappingsConfiguration.MappingManager mapper = new MappingsConfiguration.MappingManager(devSchemaQS);
        
        //For some actions, no device channel is being sent
        if (queryArgs.ContainsKey(mapper.GetValue("devChid")))
            long.TryParse(queryArgs[mapper.GetValue("devChid")], out devChid);

        TVPApi.PlatformType platform = (TVPApi.PlatformType)Enum.Parse(typeof(TVPApi.PlatformType), devType);

        info = GetAccessInfoByChannel(platform, devChid);
        info.DevSchema = devSchemaQS;
        info.initObj.UDID = queryArgs[mapper.GetValue("udid")];

        //Implement
        info.initObj.SiteGuid = "227822";
        
        GWFrontController fc = new GWFrontController(info, info.initObj.Platform);
        actionFunction actionFunc = null;
        List<object> paramsToFunc = new List<object>();

        switch (context.Request["type"])
        {
            case null:
                actionFunc = fc.GetServiceURLs;
                break;
            case "accountInfo":
                actionFunc = fc.GetAccountInfo;
                break;
            case "channel":
                actionFunc = fc.GetAllChannels;
                paramsToFunc.Add(devChid);
                break;
            case "channelInfo":
                actionFunc = fc.GetChannelInfo;
                tvmChid = long.Parse(queryArgs[mapper.GetValue("chid")]);
                paramsToFunc.Add(tvmChid);
                break;
            case "channelMedias":
                actionFunc = fc.GetChannelMedias;
                paramsToFunc.Add(long.Parse(queryArgs[mapper.GetValue("chid")]));
                bool isFullMedia = bool.Parse(System.Configuration.ConfigurationManager.AppSettings[string.Format("{0}_FullMedia", info.DevSchema)]);
                paramsToFunc.Add(isFullMedia);
                break;
            case "content":
                string titId = queryArgs[mapper.GetValue("mediaInfo")];
                string sMediaID = titId.Split('-')[0];
                string sMediaType = titId.Contains("-") ? titId.Split('-')[1] : "0";
                actionFunc = fc.GetMediaInfo;
                paramsToFunc.Add(long.Parse(sMediaID));
                paramsToFunc.Add(int.Parse(sMediaType));
                break;
            case "purchaseauth":
                actionFunc = fc.PurchaseAuth;
                break;
            case "purchaseprice":
                string titleId = queryArgs[mapper.GetValue("mediaPurchaseInfo")];
                actionFunc = fc.PurchasePrice;
                paramsToFunc.Add(int.Parse(titleId));
                break;
            case "dopurchase":
                string fileID = queryArgs[mapper.GetValue("mediaPurchaseInfo")];
                string ppvModule = queryArgs[mapper.GetValue("ppvModule")];
                string price = queryArgs[mapper.GetValue("price")];
                actionFunc = fc.DoPurchase;
                paramsToFunc.Add(int.Parse(fileID));
                paramsToFunc.Add(int.Parse(ppvModule));
                paramsToFunc.Add(double.Parse(price));
                break;
            case "searchtitles":            
                actionFunc = fc.SearchTitles;
                paramsToFunc.Add(queryArgs[mapper.GetValue("searchTerm")]);
                break;
            case "hit":                
                actionFunc = fc.DoHit;
                paramsToFunc.Add(long.Parse(queryArgs[mapper.GetValue("mediaPurchaseInfo")]));
                //XXX: fix also titID
                paramsToFunc.Add(long.Parse(queryArgs[mapper.GetValue("mediaInfo")].Split('-')[0]));
                paramsToFunc.Add(int.Parse(queryArgs[mapper.GetValue("position")]));
                break;
            case "mediamark":
                actionFunc = fc.DoMediaMark;
                paramsToFunc.Add(long.Parse(queryArgs[mapper.GetValue("mediaPurchaseInfo")]));
                //XXX: fix also titID
                paramsToFunc.Add(long.Parse(queryArgs[mapper.GetValue("mediaInfo")].Split('-')[0]));
                paramsToFunc.Add(int.Parse(queryArgs[mapper.GetValue("position")]));
                paramsToFunc.Add(queryArgs[mapper.GetValue("mediaMarkAction")]);
                break;
            case "getlastposition":
                actionFunc = fc.GetLastPosition;
                paramsToFunc.Add(int.Parse(queryArgs[mapper.GetValue("mediaInfo")].Split('-')[0]));
                break;
            default:
                break;
        }

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
        string xslt = getXSLTByDeviceName(devSchemaQS);

        // Transforming the XML to appropriate device response
        XslCompiledTransform transform = new XslCompiledTransform();
        transform.Load(new XmlTextReader(xslt, XmlNodeType.Document, null));
        XPathDocument xpd = new XPathDocument(new StringReader(serializedXML));
        using (StringWriter sr = new StringWriter())
        {
            //Due to problems changing the encoding of the resulted XML, we prepend it manually
            transform.Transform(xpd.CreateNavigator(), getXSLTArgsList(), sr);
            context.Response.Write(sr.ToString().Insert(0, "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"));
            context.Response.ContentType = "text/xml";
        }
    }

    private Dictionary<string, string> parseQS(string queryString)
    {
        Dictionary<string, string> argsDic = new Dictionary<string, string>();
        string[] pairs = queryString.Split('&');

        foreach (string pair in pairs)
        {
            string[] args = pair.Split('=');
            //This check is highly important as some devices will send a few params twice (netgem)
            if (!argsDic.ContainsKey(args[0]))
                argsDic.Add(args[0], args[1]);
        }

        return argsDic;
    }

    private XsltArgumentList getXSLTArgsList()
    {
        XsltArgumentList xslArg = new XsltArgumentList();
        xslArg.AddParam("chid", string.Empty, devChid);
        xslArg.AddParam("devtype", string.Empty, info.DevSchema);
        xslArg.AddParam("tvmChannel", string.Empty, tvmChid);
        xslArg.AddParam("udid", string.Empty, info.initObj.UDID);

        return xslArg;
    }

    private string getXSLTByDeviceName(string devName)
    {
        return File.ReadAllText(HttpContext.Current.Server.MapPath(".") + "/Transformations/" + devName + ".xslt");
    }

    //XXX: Make config
    private GWFrontController.ApiAccessInfo GetAccessInfoByChannel(TVPApi.PlatformType devType, long devChid)
    {
        switch (devType)
        {
            case TVPApi.PlatformType.ConnectedTV:
            case TVPApi.PlatformType.STB:
                string provider = string.Empty;
                switch (devChid)
                {
                    case 0:
                        provider = "none";
                        break;
                    case 51:
                        provider = "orange";
                        break;
                    case 200:
                    case 800:
                        provider = "novebox";
                        break;
                    case 901:
                    case 801:
                    case 201:
                    case 202:
                    case 203:
                    case 204:
                        provider = "ipvision";
                        break;
                    default:
                        break;
                }
                return parseAccessDataByProvider(provider, devType);                        
            default:
                break;
        }

        throw new Exception("Dev channel was not found");
    }

    private GWFrontController.ApiAccessInfo parseAccessDataByProvider(string provider, TVPApi.PlatformType devType)
    {
        switch (provider.ToLower())
        {
            case "novebox":
                return new GWFrontController.ApiAccessInfo() { GroupID = 93, initObj = new TVPApi.InitializationObject() { ApiUser = "tvpapi_93", ApiPass = "11111", Platform = devType } };
            case "none":
            case "ipvision":
                return new GWFrontController.ApiAccessInfo() { GroupID = 125, initObj = new TVPApi.InitializationObject() { ApiUser = "tvpapi_125", ApiPass = "11111", Platform = devType } };
            case "turkcell":
                return new GWFrontController.ApiAccessInfo() { GroupID = 131, initObj = new TVPApi.InitializationObject() { ApiUser = "tvpapi_131", ApiPass = "11111", Platform = devType } };
            default:
                throw new Exception("provider not found");
        }
    }

    public bool IsReusable
    {
        get
        {
            return true;
        }
    }
}