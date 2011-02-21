using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVPApi;
using System.Xml;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using Tvinci.Helpers;

public partial class Gateways_RSSGateway : System.Web.UI.Page
{
   
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            Logger.Logger.Log("PTV Request ", Request.RawUrl, "TVPApi");
            string opCode = Request.QueryString["op"];
            if (!string.IsNullOrEmpty(opCode))
            {
                switch (opCode)
                {
                    case "GetPage":
                        {
                            if (!string.IsNullOrEmpty(Request.QueryString["id"]))
                            {
                                long pageID = Convert.ToInt64(Request.QueryString["id"]);
                                GetPageRSS(pageID);
                            }
                            break;
                        }
                    case "GetMainGallery":
                        {
                            if (!string.IsNullOrEmpty(Request.QueryString["id"]) && !string.IsNullOrEmpty(Request.QueryString["pageid"]) && !string.IsNullOrEmpty(Request.QueryString["sid"]))
                            {
                                long galleryID = Convert.ToInt64(Request.QueryString["id"]);
                                string sID = Request.QueryString["sid"];
                                long pageID = Convert.ToInt64(Request.QueryString["pageid"]);
                                GetGalleryRSS(galleryID, pageID, sID);
                            }
                            break;
                        }
                    case "GetFavoriteItems":
                        if (!string.IsNullOrEmpty(Request.QueryString["sid"]) && !string.IsNullOrEmpty(Request.QueryString["itemtype"]))
                        {
                            UserItemType itemType = ((UserItemType)Convert.ToInt32(Request.QueryString["itemtype"]));
                            string sID = Request.QueryString["sid"];
                            string height = Request.QueryString["height"];
                            string width = Request.QueryString["width"];

                            GetFavoritesRSS(sID, height, width, itemType);
                            //GetGalleryRSS(galleryID, pageID);
                        }
                        break;
                    
                    default:
                        break;
                }
            }
        }
    }

    private void GetPageRSS(long pageID)
    {
        Response.Clear();
        Response.ContentType = "text/xml";
        Service service = new Service();
        InitializationObject initObj = GetInitObj();
        string userName = "tvpapi_93";
        string pass = "11111";
        service.GetSiteMap(initObj, "tvpapi_93", "11111");
        PageContext page = service.GetPage(initObj, userName, pass, pageID, false, false);

        XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, System.Text.Encoding.UTF8);
        RSSWriter rssWriter = new RSSWriter(writer);
        
        rssWriter.WritePageRSS(page, string.Concat(LinkHelper.ParseURL(ConfigurationManager.AppSettings["RSSGatewayPath"]),"PTVGateway.aspx?op=GetMainGallery&id={0}&pageid={1}"));

        writer.Flush();

        writer.Close();
        
        
        Response.ContentEncoding = System.Text.Encoding.UTF8;

        HttpContext.Current.ApplicationInstance.CompleteRequest();

        Response.End();
    }

    private void GetFavoritesRSS(string guid, string height, string width, UserItemType type)
    {
        Response.Clear();
        Response.ContentType = "text/xml";
        Service service = new Service();
        InitializationObject initObj = GetInitObj();
        initObj.Locale = new Locale();
        initObj.Locale.SiteGuid = guid;
        string userName = "tvpapi_93";
        string pass = "11111";
        service.GetSiteMap(initObj, userName, pass);
        List<Media> userItems = service.GetUserItems(initObj, userName, pass, type, 0, "full", 20, 0);
        

        XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, System.Text.Encoding.UTF8);
        RSSWriter rssWriter = new RSSWriter(writer);

        rssWriter.WriteFavoritesGallery(userItems, height, width);

        writer.Flush();

        writer.Close();

        Response.ContentEncoding = System.Text.Encoding.UTF8;

        HttpContext.Current.ApplicationInstance.CompleteRequest();

        Response.End();
    }

    private void GetGalleryRSS(long galleryID, long pageID, string sID)
    {
        Response.Clear();
        Response.ContentType = "text/xml";
        Service service = new Service();
        InitializationObject initObj = GetInitObj();
        string userName = "tvpapi_93";
        string pass = "11111";
        PageGallery pg = service.GetGallery(initObj, userName, pass, galleryID, pageID);

        XmlTextWriter writer = new XmlTextWriter(Response.OutputStream, System.Text.Encoding.UTF8);
        RSSWriter rssWriter = new RSSWriter(writer);

        rssWriter.WriteGalleryRSS(pg, sID);

        writer.Flush();

        writer.Close();

        Response.ContentEncoding = System.Text.Encoding.UTF8;

        HttpContext.Current.ApplicationInstance.CompleteRequest();

        Response.End();

    }

    protected InitializationObject GetInitObj()
    {
        InitializationObject initObj = new InitializationObject();
        initObj.Platform = PlatformType.STB;
        return initObj;
    }

    protected string GetWSPass()
    {
        return "11111";
    }

    protected string GetWSUser()
    {
        return "tvpapi_93";
    }

    
}
