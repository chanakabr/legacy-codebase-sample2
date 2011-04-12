using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVPApi;
using System.Web.Script.Serialization;
using System.Text;
using Logger;
using Tvinci.Data.DataLoader;
using TVPApiServices;

public partial class Gateways_ActivaGateway : BaseGateway
{
    private MediaService m_mediaService = new MediaService();
    private SiteService m_siteService = new SiteService();

    protected void Page_Load(object sender, EventArgs e)
    {
        //TODO: Logger.Logger.Log("Activa Request ", Request.Url.ToString(), "TVPApi");
        long itemsCount = 0;
        string retVal = string.Empty;
        string action = Request.QueryString["Action"];
        string id = Request.QueryString["id"];
        string items = Request.QueryString["items"];
        string index = Request.QueryString["index"];
        string broadcasterName = Request.QueryString["broadcasterName"];
        string wsUser = string.Empty;
        string wsPass = string.Empty;
        int groupID = GetGroupIDByBroadcasterName(broadcasterName, ref wsUser, ref wsPass);
        int nIndex = 0;
        if (!string.IsNullOrEmpty(index))
        {
            nIndex = int.Parse(index);
        }
        int nItems = 0;
        if (!string.IsNullOrEmpty(items))
        {
            nItems = int.Parse(items);
        }

        string callBack = Request.QueryString["callback"];
        //Init();

        //TODO: Logger.Logger.Log("Start Call ", "Start query - " + wsUser + " " + wsPass, "TVPApi");

        // check if request in cache and write it
        object retObj = HttpRuntime.Cache.Get(HttpContext.Current.Request.Url.ToString().ToLower());
        if (retObj != null && retObj is string)
        {
            //TODO: Logger.Logger.Log("Response from cache", "query - " + wsUser + " " + wsPass, "TVPApi");
            
            Response.Clear();
            Response.Write(retObj.ToString());
            Response.End();
            return;
        }

        try
        {
            m_siteService.GetSiteMap(GetInitObj(), wsUser, wsPass);
            //SiteMapManager.GetInstance.GetMember(groupID, PlatformType.STB);

            if (!string.IsNullOrEmpty(action))
            {
                switch (action.ToLower())
                {
                    case "getfunctions": //<--
                        {
                            //PageContext page = tvpapiService.GetPageByToken(GetInitObj(), wsUser, wsPass, 
                            PageContext page = PageDataHelper.GetPageContextByToken(GetInitObj(), groupID, Pages.HomePage, false, false);
                            if (page != null)
                            {
                                foreach (PageGallery pg in page.MainGalleries)
                                {
                                    retObj = pg.GalleryItems;
                                    break;
                                }
                            }
                            //page.MainGalleries
                            break;
                        }
                    case "getpage": //<--
                        {
                            string tokenStr = Request.QueryString["Token"];
                            string level = Request.QueryString["level"];

                            Pages token = ParseStringToToken(tokenStr);
                            PageContext page = m_siteService.GetPageByToken(GetInitObj(), wsUser, wsPass, token, false, false);
                            //PageContext page = PageDataHelper.GetPageContextByToken(GetInitObj(), groupID, token, false, false);
                            if (level.Equals("3"))
                            {
                                retObj = page;
                            }
                            else if (level.Equals("1"))
                            {
                                PageGallery pg = page.MainGalleries.FirstOrDefault();
                                if (pg != null)
                                {
                                    GalleryItem gi = pg.GalleryItems.FirstOrDefault();
                                    if (gi != null)
                                    {
                                        retObj = gi;
                                    }
                                }
                            }
                            break;
                        }
                    case "getfunctionassets": //<--
                        {
                            List<Media> mediaList = m_mediaService.GetChannelMediaListWithMediaCount(GetInitObj(), wsUser, wsPass, long.Parse(id), "full", int.Parse(items), int.Parse(index), ref itemsCount);
                            //List<Media> mediaList = MediaHelper.GetChannelMediaList(GetInitObj(), long.Parse(id), "full", nItems, nIndex, groupID);
                            retObj = new List<Media>(mediaList);
                            break;
                        }
                    case "getrelatedmedia": //<--
                        {

                            List<Media> mediaList = m_mediaService.GetRelatedMediaWithMediaCount(GetInitObj(), wsUser, wsPass, int.Parse(id), 0, "full", int.Parse(items), int.Parse(index), ref itemsCount);
                            retObj = new List<Media>(mediaList);

                            break;
                        }
                    case "search": //<--
                        {
                            string val = Request.QueryString["query"];

                            List<Media> mediaList = m_mediaService.SearchMediaWithMediaCount(GetInitObj(), wsUser, wsPass, val, 0, "full", int.Parse(items), int.Parse(index), (OrderBy)TVPApi.OrderBy.Added, ref itemsCount);
                            //List<Media> mediaList = MediaHelper.SearchMedia(GetInitObj(), 0, val, "full", int.Parse(items), 0, groupID, groupID, (int)TVPApi.OrderBy.ABC);
                            retObj = new List<Media>(mediaList);

                            break;
                        }
                    case "typingsearch": //<--
                        {
                            string val = Request.QueryString["query"];
                            List<string> autos = MediaHelper.GetAutoCompleteList(groupID, PlatformType.STB, groupID);
                            //List<Media> mediaList = MediaHelper.SearchMedia(GetInitObj(), 0, val, "full", int.Parse(items), 0, 121, 122, (int)TVPApi.OrderBy.ABC);
                            //retObj = mediaList;
                            List<string> tmpAutos = new List<string>(autos);
                            retObj = ParseAutoCompleteList(tmpAutos, val);
                            retVal = (string)retObj;
                            retVal = string.Format("{0}({1})", callBack, retVal);
                            break;
                        }
                    case "getcatchup": //<--
                        {
                            string dateStr = Request.QueryString["day"].ToString();
                            List<Media> mediaList = m_mediaService.SearchMediaByMetaWithMediaCount(GetInitObj(), wsUser, wsPass, "Date", dateStr, 0, "full", int.Parse(items), int.Parse(index), OrderBy.Added, ref itemsCount);
                            retObj = new List<Media>(mediaList);
                            // List<Media> mediaList = MediaHelper.SearchMediaByMeta(GetInitObj(), 0, "Date", dateStr, "full", int.Parse(items), int.Parse(index), groupID, (int)(TVPPro.SiteManager.Context.Enums.eOrderBy.Added));
                            // caMod.get
                            break;
                        }
                    case "GetMediaFilse":
                        {
                            ca.module caMod = new ca.module();
                            // caMod.get
                            break;
                        }
                    default:
                        break;
                }
            }


            if (retObj != null && !(retObj is string))
            {
                retVal = ParseObject(retObj, groupID, nItems, nIndex, itemsCount);
                retVal = string.Format("{0}({1})", callBack, retVal);
                //TODO: Logger.Logger.Log("Activa Response ", "Request :" + Request.Url.ToString() + " Response :" + retVal, "TVPApi");
            }
        }
        catch (Exception ex)
        {
            ActivaErrorObj errObj = new ActivaErrorObj();

            errObj.status = string.Format("Error on server : {0}", ex.Message);
            retVal = string.Format("{0}({1})", callBack, CreateJson(errObj));
            //TODO: Logger.Logger.Log("Activa Error on request ", "Request :" + Request.Url.ToString() + " Error: " + ex.Message + " " + ex.StackTrace, "TVPApiExceptions");
        }

        HttpRuntime.Cache.Insert(HttpContext.Current.Request.Url.ToString().ToLower(), retVal, null, DateTime.Now.AddHours(24), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Low, null);

        Response.Clear();
        Response.Write(retVal);
        Response.End();
    }

    private int GetGroupIDByBroadcasterName(string broadcasterName, ref string wsUser, ref string wsPass)
    {
        int retVal = 0;
        switch (broadcasterName.ToLower())
        {
            case "tele5":
                {
                    retVal = 122;
                    wsUser = "tvpapi_122";
                    wsPass = "11111";
                    break;
                }
            case "whitelabel":
                {
                    retVal = 123;
                    wsUser = "tvpapi_123";
                    wsPass = "11111";
                    break;
                }
            case "whitelabel2":
                {
                    retVal = 124;
                    wsUser = "tvpapi_124";
                    wsPass = "11111";
                    break;
                }

            case "novetest":
                {
                    retVal = 93;
                    wsUser = "tvpapi_93";
                    wsPass = "11111";
                    break;
                }
            default:
                {
                    retVal = 122;
                    wsUser = "tvpapi_122";
                    wsPass = "11111";
                    break;
                }
        }
        return retVal;
    }



    protected override InitializationObject GetInitObj()
    {
        InitializationObject retVal = new InitializationObject();
        retVal.Platform = PlatformType.STB;
        //Locale locale = new Locale();
        //locale.LocaleLanguage = "es";
        //retVal.Locale = locale;
        return retVal;
    }

    private string ParseAutoCompleteList(List<string> lstResponse, string preFix)
    {
        TVPApi.AbertisJSONParser.AutoCompleteList retVal = new AbertisJSONParser.AutoCompleteList();
        foreach (String sTitle in lstResponse)
        {
            if (sTitle.ToLower().StartsWith(preFix.ToLower()))
            {
                if (retVal.content == null)
                {
                    retVal.content = new List<AbertisJSONParser.AutoCompleteObj>();
                }
                TVPApi.AbertisJSONParser.AutoCompleteObj obj = new AbertisJSONParser.AutoCompleteObj();
                obj.choice = sTitle;
                retVal.content.Add(obj);
            }
        }
        return CreateJson(retVal);
    }


    private Pages ParseStringToToken(string tokenStr)
    {
        Pages retVal = Pages.UnKnown;
        switch (tokenStr.ToLower())
        {
            case "homepage":
                {
                    retVal = Pages.HomePage;
                    break;
                }
            case "vod":
                {
                    retVal = Pages.ShowPage;
                    break;
                }
            default:
                retVal = Pages.HomePage;
                break;
        }
        return retVal;
    }

    protected override string GetWSPass()
    {
        return "11111";
    }

    protected override string GetWSUser()
    {
        return "tvp_121";
    }



    private string CreateJson(object obj)
    {
        StringBuilder sb = new StringBuilder();
        JavaScriptSerializer jsSer = new JavaScriptSerializer();
        jsSer.Serialize(obj, sb);
        return sb.ToString();
    }

    //private void Init()
    //{
    //    HttpContext.Current.Items["GroupID"] = 121;
    //    HttpContext.Current.Items["Platform"] = PlatformType.STB;
    //    HttpContext.Current.Items["IsShared"] = false;
    //}

    public class ActivaErrorObj
    {
        public string status;
        public object content;
    }


}
