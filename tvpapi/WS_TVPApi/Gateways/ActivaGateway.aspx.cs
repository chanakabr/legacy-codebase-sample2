using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVPApi;
using System.Web.Script.Serialization;
using System.Text;
using Tvinci.Data.DataLoader;
using System.Configuration;
using KLogMonitor;
using System.Reflection;

public partial class Gateways_ActivaGateway : BaseGateway
{
    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
    static ILoaderCache m_dataCaching = LoaderCacheLite.Current;

    protected void Page_Load(object sender, EventArgs e)
    {
        //Logger.Logger.Log("Activa Request ", Request.Url.ToString(), "TVPApi");        
        long itemsCount = 0;
        string retVal = string.Empty;
        string action = Request.QueryString["Action"];
        string id = Request.QueryString["id"];
        string items = Request.QueryString["items"];
        string index = Request.QueryString["index"];
        string broadcasterName = Request.QueryString["broadcasterName"];
        int groupID = GetGroupIDByBroadcasterName(broadcasterName);

        int nIndex = 0;
        int nItems = 0;
        int nID = 0;

        int.TryParse(index, out nIndex);
        int.TryParse(items, out nItems);
        int.TryParse(id, out nID);

        string callBack = Request.QueryString["callback"];
        //Init();

        //        Logger.Logger.Log("Start Call ", "Start query - " + WsUserName + " " + WsUserName, "TVPApi");

        // check if request in cache and write it
        object retObj = HttpRuntime.Cache.Get(HttpContext.Current.Request.Url.ToString().ToLower());
        if (retObj != null && retObj is string)
        {
            //          Logger.Logger.Log("Response from cache", "query - " + WsUserName + " " + WsUserName, "TVPApi");

            Response.Clear();
            Response.Write(retObj.ToString());
            Response.End();
            return;
        }

        try
        {
            m_SiteService.GetSiteMap(GetInitObj());
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
                            PageContext page = m_SiteService.GetPageByToken(GetInitObj(), token, false, false);
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
                            List<Media> mediaList = m_MediaService.GetChannelMediaListWithMediaCount(GetInitObj(), nID, "full", nItems, nIndex, ref itemsCount);
                            //List<Media> mediaList = MediaHelper.GetChannelMediaList(GetInitObj(), long.Parse(id), "full", nItems, nIndex, groupID);
                            retObj = mediaList;
                            break;
                        }
                    case "getrelatedmedia": //<--
                        {
                            List<Media> mediaList = m_MediaService.GetRelatedMediaWithMediaCount(GetInitObj(), nID, 0, "full", nItems, nIndex, ref itemsCount);
                            retObj = mediaList;

                            break;
                        }
                    case "search": //<--
                        {
                            string val = Request.QueryString["query"];

                            List<Media> mediaList = m_MediaService.SearchMediaWithMediaCount(GetInitObj(), val, 0, "full", nItems, nIndex, (OrderBy)TVPApi.OrderBy.Added, ref itemsCount);
                            //List<Media> mediaList = MediaHelper.SearchMedia(GetInitObj(), 0, val, "full", int.Parse(items), 0, groupID, groupID, (int)TVPApi.OrderBy.ABC);
                            retObj = mediaList;

                            break;
                        }
                    case "typingsearch": //<--
                        {
                            string val = Request.QueryString["query"];
                            List<string> autos = null;// MediaHelper.GetAutoCompleteList(groupID, PlatformType.STB, groupID, null);
                            //List<Media> mediaList = MediaHelper.SearchMedia(GetInitObj(), 0, val, "full", int.Parse(items), 0, 121, 122, (int)TVPApi.OrderBy.ABC);
                            //retObj = mediaList;
                            retObj = ParseAutoCompleteList(autos, val);
                            retVal = (string)retObj;
                            retVal = string.Format("{0}({1})", callBack, retVal);
                            break;
                        }
                    case "getcatchup": //<--
                        {
                            string dateStr = Request.QueryString["day"].ToString();
                            List<Media> mediaList = m_MediaService.SearchMediaByMetaWithMediaCount(GetInitObj(), "Date", dateStr, 0, "full", nItems, nIndex, OrderBy.Added, ref itemsCount);
                            retObj = mediaList;
                            // List<Media> mediaList = MediaHelper.SearchMediaByMeta(GetInitObj(), 0, "Date", dateStr, "full", int.Parse(items), int.Parse(index), groupID, (int)(TVPPro.SiteManager.Context.Enums.eOrderBy.Added));
                            // caMod.get
                            break;
                        }
                    case "GetMediaFilse":
                        {
                            //ca.module caMod = new ca.module();
                            // caMod.get
                            break;
                        }
                    default:
                        break;
                }
            }


            if (retObj != null && !(retObj is string))
            {
                retVal = ParseObject(retObj, groupID, nItems, nIndex, itemsCount, PlatformType.STB);
                retVal = retVal.Replace("120X90", "160X90");
                retVal = string.Format("{0}({1})", callBack, retVal);
                //        Logger.Logger.Log("Activa Response ", "Request :" + Request.Url.ToString() + " Response :" + retVal, "TVPApi");
            }
        }
        catch (Exception ex)
        {
            ActivaErrorObj errObj = new ActivaErrorObj();

            errObj.status = string.Format("Error on server : {0}", ex.Message);
            retVal = string.Format("{0}({1})", callBack, CreateJson(errObj));
            //      Logger.Logger.Log("Activa Error on request ", "Request :" + Request.Url.ToString() + " Error: " + ex.Message + " " + ex.StackTrace, "TVPApiExceptions");
        }

        HttpRuntime.Cache.Insert(HttpContext.Current.Request.Url.ToString().ToLower(), retVal, null, DateTime.Now.AddHours(24), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Low, null);

        Response.Clear();
        Response.Write(retVal);

        try
        {
            if (retVal.ToLower().Contains(@"""videourl"":"""""))
            {
                SendSMS(string.Format("{0}->TVPApi::{1},{2}:{3} request has empty VideoURL value. Application pool Restarted", Environment.MachineName, broadcasterName, action, Request.QueryString["day"]));
                //            Logger.Logger.Log("TVPApi application pool retarted", "videoURL empty value", "TVPApi");
                HttpRuntime.UnloadAppDomain();
            }
        }
        catch (Exception ex)
        {
            logger.Error("", ex);
        }

        Response.End();
    }

    private void SendSMS(string sMessage)
    {
        try
        {
            string sUserName = ConfigurationManager.AppSettings["SMS_WS_UN"];
            string sPassword = ConfigurationManager.AppSettings["SMS_WS_PW"];
            object[] sPhones = ConfigurationManager.AppSettings["SMS_WS_PHONES"].Split(';');
            object[] sMails = ConfigurationManager.AppSettings["SMS_WS_MAILS"].Split(';');
            string sSenderName = "TVPApi";

            if (sPhones.Length > 0)
            {
                string sSMSMessage = sMessage;
                //if (sMessage.Length > 65)
                //sSMSMessage = sMessage.Substring(0, 65) + "...";
                il.co.smscenter.www.SendSMS sender = new il.co.smscenter.www.SendSMS();
                sender.Url = "http://www.smscenter.co.il/Web/WebServices/SendMessage.asmx";
                il.co.smscenter.www.SendMessageReturnValues ret = sender.SendMessages(sUserName, sPassword, sSenderName,
                    sPhones, sSMSMessage, sMails, il.co.smscenter.www.SMSOperation.Push, "", il.co.smscenter.www.DeliveryReportMask.MessageExpired, 0, 60);

                //      Logger.Logger.Log("SMS sent - returnd: " + ret.ToString(), sMessage, "SMSer");
                //return true;
            }
            //return false;
        }
        catch (Exception ex)
        {
            logger.Error("SMS send", ex);
            //return false;
        }
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
