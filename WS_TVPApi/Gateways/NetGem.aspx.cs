using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using TVPApi;
using System.Web.Script.Serialization;
using System.Text;
using Logger;
using Tvinci.Data.DataLoader;
using System.Xml;
using TVPPro.SiteManager.Helper;
using System.Net;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.SiteManager.DataEntities;
using System.Data;
using TVPPro.SiteManager.Context;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPApiModule.DataLoaders;
using TVPApiModule;

public partial class Gateways_NetGem : BaseGateway
{
    static ILoaderCache m_dataCaching = LoaderCacheLite.Current;
    static int counter = 0;
    //XXX: Unify in one class
    public enum eChannels { Novebox = 50, Orange = 51, Ipvision = 901 };

    protected void Page_Load(object sender, EventArgs e)
    {
        Logger.Logger.Log("Netgem Request ", Request.Url.ToString(), "TVPApi");

        Service tvpapiService = new Service();
        long itemsCount = 0;
        string retVal = string.Empty;
        string action = Request.QueryString["Action"];
        string id = Request.QueryString["id"];
        string items = Request.QueryString["items"];
        string index = Request.QueryString["index"];
        string broadcasterName = Request.QueryString["broadcasterName"];
        string wsUser = string.Empty;
        string wsPass = string.Empty;

        broadcasterName = "novetest";
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
        object retObj = null;
        //Init();

        #region Marked
        // check if request in cache and write it 
        //if (m_dataCaching.TryGetData<object>(HttpContext.Current.Request.Url.ToString().ToLower(), out retObj))
        //{
        //    if (retObj != null && retObj is string)
        //    {
        //        Response.Clear();
        //        Response.Write(retObj.ToString());
        //        Response.Expires = -1;
        //        Response.End();
        //        return;
        //    }
        //}

        //Logger.Logger.Log("Start Call ", "Start query - " + wsUser + " " + wsPass, "TVPApi");
        //try
        //{
        //    tvpapiService.GetSiteMap(GetInitObj(), wsUser, wsPass);
        //    //SiteMapManager.GetInstance.GetMember(groupID, PlatformType.STB);

        //    if (!string.IsNullOrEmpty(action))
        //    {
        //        switch (action.ToLower())
        //        {
        //            case "getfunctions": //<--
        //                {
        //                    //PageContext page = tvpapiService.GetPageByToken(GetInitObj(), wsUser, wsPass, 
        //                    PageContext page = PageDataHelper.GetPageContextByToken(GetInitObj(), groupID, Pages.HomePage, false, false);
        //                    if (page != null)
        //                    {
        //                        foreach (PageGallery pg in page.MainGalleries)
        //                        {
        //                            retObj = pg.GalleryItems;
        //                            break;
        //                        }
        //                    }
        //                    //page.MainGalleries
        //                    break;
        //                }
        //            case "getpage": //<--
        //                {
        //                    string tokenStr = Request.QueryString["Token"];
        //                    string level = Request.QueryString["level"];

        //                    Pages token = ParseStringToToken(tokenStr);
        //                    PageContext page = tvpapiService.GetPageByToken(GetInitObj(), wsUser, wsPass, token, false, false);
        //                    //PageContext page = PageDataHelper.GetPageContextByToken(GetInitObj(), groupID, token, false, false);
        //                    if (level.Equals("3"))
        //                    {
        //                        retObj = page;
        //                    }
        //                    else if (level.Equals("1"))
        //                    {
        //                        PageGallery pg = page.MainGalleries.FirstOrDefault();
        //                        if (pg != null)
        //                        {
        //                            GalleryItem gi = pg.GalleryItems.FirstOrDefault();
        //                            if (gi != null)
        //                            {
        //                                retObj = gi;
        //                            }
        //                        }
        //                    }
        //                    break;
        //                }
        //            case "getfunctionassets": //<--
        //                {
        //                    List<Media> mediaList = tvpapiService.GetChannelMediaListWithMediaCount(GetInitObj(), wsUser, wsPass, long.Parse(id), "full", int.Parse(items), int.Parse(index), ref itemsCount);
        //                    //List<Media> mediaList = MediaHelper.GetChannelMediaList(GetInitObj(), long.Parse(id), "full", nItems, nIndex, groupID);
        //                    retObj = mediaList;
        //                    break;
        //                }
        //            case "getrelatedmedia": //<--
        //                {
        //                    List<Media> mediaList = tvpapiService.GetRelatedMediaWithMediaCount(GetInitObj(), wsUser, wsPass, int.Parse(id), 0, "full", int.Parse(items), int.Parse(index), ref itemsCount);
        //                    retObj = mediaList;

        //                    break;
        //                }
        //            case "search": //<--
        //                {
        //                    string val = Request.QueryString["query"];

        //                    List<Media> mediaList = tvpapiService.SearchMediaWithMediaCount(GetInitObj(), wsUser, wsPass, val, 0, "full", int.Parse(items), int.Parse(index), (OrderBy)TVPApi.OrderBy.ABC, ref itemsCount);
        //                    //List<Media> mediaList = MediaHelper.SearchMedia(GetInitObj(), 0, val, "full", int.Parse(items), 0, groupID, groupID, (int)TVPApi.OrderBy.ABC);
        //                    retObj = mediaList;

        //                    break;
        //                }
        //            case "typingsearch": //<--
        //                {
        //                    string val = Request.QueryString["query"];
        //                    List<string> autos = MediaHelper.GetAutoCompleteList(groupID, PlatformType.STB, groupID);
        //                    //List<Media> mediaList = MediaHelper.SearchMedia(GetInitObj(), 0, val, "full", int.Parse(items), 0, 121, 122, (int)TVPApi.OrderBy.ABC);
        //                    //retObj = mediaList;
        //                    retObj = ParseAutoCompleteList(autos, val);
        //                    retVal = (string)retObj;
        //                    retVal = string.Format("{0}({1})", callBack, retVal);
        //                    break;
        //                }
        //            case "getcatchup": //<--
        //                {
        //                    string dateStr = Request.QueryString["day"].ToString();
        //                    List<Media> mediaList = tvpapiService.SearchMediaByMetaWithMediaCount(GetInitObj(), wsUser, wsPass, "Date", dateStr, 0, "full", int.Parse(items), int.Parse(index), OrderBy.Added, ref itemsCount);
        //                    retObj = mediaList;
        //                    // List<Media> mediaList = MediaHelper.SearchMediaByMeta(GetInitObj(), 0, "Date", dateStr, "full", int.Parse(items), int.Parse(index), groupID, (int)(TVPPro.SiteManager.Context.Enums.eOrderBy.Added));
        //                    // caMod.get
        //                    break;
        //                }
        //            case "GetMediaFilse":
        //                {
        //                    ca.module caMod = new ca.module();
        //                    // caMod.get
        //                    break;
        //                }
        //            default:
        //                break;
        //        }
        //    }


        //    if (retObj != null && !(retObj is string))
        //    {
        //        retVal = ParseObject(retObj, groupID, nItems, nIndex, itemsCount);
        //        retVal = string.Format("{0}({1})", callBack, retVal);
        //        Logger.Logger.Log("Activa Response ", "Request :" + Request.Url.ToString() + " Response :" + retVal, "TVPApi");
        //    }
        //}
        //catch (Exception ex)
        //{
        //    ActivaErrorObj errObj = new ActivaErrorObj();

        //    errObj.status = string.Format("Error on server : {0}", ex.Message);
        //    retVal = string.Format("{0}({1})", callBack, CreateJson(errObj));
        //    Logger.Logger.Log("Activa Error on request ", "Request :" + Request.Url.ToString() + " Error: " + ex.Message + " " + ex.StackTrace, "TVPApiExceptions");
        //}
        #endregion

        string sType = Request.QueryString["type"];
        string titId = Request.QueryString["titId"];
        if (!string.IsNullOrEmpty(titId))
        {
            sType = "content";
        }

        string sChID = Request.QueryString["chid"];

        PageData pd = SiteMapManager.GetInstance.GetPageData(groupID, PlatformType.STB);

        PageContext pc = pd.GetPageByID("es", 69); //

        DateTime objUTC = DateTime.Now.ToUniversalTime();
        long epoch = (objUTC.Ticks - 62135596800000000) / 10000;
        StringWriter sw = new StringWriter();
        XmlTextWriter XTM = new XmlTextWriter(sw);
        XTM.WriteStartDocument();

        switch (sType)
        {
            case "account":
                ///XXX: Fix
                if (Request.QueryString["identity"] == "00043047454B")
                {
                    Response.Redirect(string.Concat("~/gateways/netgem_ipvision.aspx", Request.Url.Query), true);
                }
                //bool bLoginSuccess = new UsersServiceEx(groupID, PlatformType.STB.ToString()).SignIn("idow@gmail.com", "eliron27");

                string sSiteGuid = new ApiUsersService(groupID, PlatformType.STB).SignIn("adina@tvinci.com", "eliron27");

                XTM.WriteStartElement("account");
                XTM.WriteStartElement("information");
                XTM.WriteStartElement("distributor");
                XTM.WriteCData("");
                XTM.WriteEndElement(); // distributor
                XTM.WriteElementString("contract", sSiteGuid);
                XTM.WriteElementString("date", "05/09/2010 08:39");

                UserResponseObject userResponseObject = new ApiUsersService(groupID, PlatformType.STB).GetUserData(sSiteGuid);

                if (userResponseObject != null && userResponseObject.m_user != null && userResponseObject.m_user.m_oBasicData != null)
                {
                    XTM.WriteElementString("email", userResponseObject.m_user.m_oBasicData.m_sEmail);
                }
                XTM.WriteElementString("pay", "CreditCard");
                XTM.WriteElementString("logo", "");

                XTM.WriteEndElement(); // information

                PermittedMediaContainer[] MediaPermitedItems = new ApiConditionalAccessService(groupID, PlatformType.STB).GetUserPermittedItems(sSiteGuid);

                XTM.WriteStartElement("streams");
                if (MediaPermitedItems != null && MediaPermitedItems.Count() > 0)
                {
                    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, PlatformType.STB).GetTVMAccountByAccountType(AccountType.Regular);
                    dsItemInfo ItemInfo = new APITVMRentalMultiMediaLoader(account.TVMUser, account.TVMPass, "full", 1) { GroupID = groupID, Platform = PlatformType.STB, MediasIdCotainer = MediaPermitedItems, SearchTokenSignature = GetMediasWithSeperator(MediaPermitedItems) }.Execute(); // Type 1 means bring all types

                    for (int i = 0; i < ItemInfo.Item.Rows.Count; i++)
                    {
                        XTM.WriteStartElement("stream");
                        XTM.WriteAttributeString("id", string.Concat(ItemInfo.Item[i].ID, "-", ItemInfo.Item[i].MediaTypeID));
                        XTM.WriteStartElement("title");
                        XTM.WriteCData(ItemInfo.Item[i].Title);
                        XTM.WriteEndElement();//

                        int iFileID = int.Parse(ItemInfo.Item[i].FileID);

                        Dictionary<int, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.MediaFileItemPricesContainer> dictPrices = new ApiConditionalAccessService(groupID, PlatformType.STB).GetItemsPrice(new int[] { iFileID }, sSiteGuid, false);

                        if (dictPrices != null && dictPrices.Keys.Contains(iFileID) && dictPrices[iFileID].m_oItemPrices != null && dictPrices[iFileID].m_oItemPrices.Length > 0)
                        {
                            XTM.WriteElementString("price", dictPrices[iFileID].m_oItemPrices[0].m_oPrice.m_dPrice.ToString());
                            XTM.WriteElementString("realprice", dictPrices[iFileID].m_oItemPrices[0].m_oFullPrice.m_dPrice.ToString());
                        }
                        else
                        {
                            XTM.WriteElementString("price", "1.99");
                            XTM.WriteElementString("realprice", "0.99");
                        }

                        XTM.WriteElementString("date", ItemInfo.Item[i].PurchaseDate.ToString());
                        XTM.WriteEndElement();//stream
                    }

                }
                XTM.WriteEndElement(); // streams
                XTM.WriteEndElement(); // account

                XTM.WriteEndDocument();

                break;
            case "content":
                try
                {
                    RedirectGWToChannel(titId.Split('-')[2]);
                }
                catch (Exception)
                {
                    //ignore cached items
                }
                break;
            case "purchase":
                // XXX: make this "smart" against the DMS
                XTM.WriteStartElement("purchase");
                XTM.WriteElementString("state", "authentication");
                XTM.WriteElementString("challenge", "b252eb9a533c8ae37a462a267e1f2fa9");
                XTM.WriteEndElement();
                XTM.WriteEndDocument();
                break;
            case "purchasestatus":
                RedirectGWToChannel(Request["vtiId"].Split('-')[4]);
                break;
            case "purchaseConfirmation":

                // Reading the XML postdata so we can get the hmac
                //byte[] buff = new byte[Request.ContentLength];
                //Request.InputStream.Read(buff, 0, Request.ContentLength);
                //string postData = System.Text.UTF8Encoding.UTF8.GetString(buff);
                //XmlDocument postXML = new XmlDocument();
                //postXML.LoadXml(postData);
                //string hmac = postXML.GetElementsByTagName("hmac")[0].InnerText;                

                string mac = Request.QueryString["identity"];
                //HttpWebRequest HttpWReq = (HttpWebRequest)WebRequest.Create(string.Format(@"http://request-dms.netboxtv.netgem.com/challenge/{0}/b252eb9a533c8ae37a462a267e1f2fa9/{1}", mac, hmac));
                //HttpWReq.Method = "POST";
                //HttpWReq.ContentType = "application/x-www-form-urlencoded";

                //string xmlPost = string.Format("<data><user>{0}</user><challenge>b252eb9a533c8ae37a462a267e1f2fa9</challenge><hmac>{1}</hmac></data>", mac, hmac);
                //byte[] byteArray = Encoding.UTF8.GetBytes(xmlPost);
                //HttpWReq.ContentLength = byteArray.Length;

                //using (Stream stream = HttpWReq.GetRequestStream())
                //{
                //    stream.Write(byteArray, 0, byteArray.Length);
                //    stream.Close();

                // Authentication against the DMS
                //HttpWebResponse resp = (HttpWebResponse)HttpWReq.GetResponse();
                //StreamReader responseReader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                //string res = responseReader.ReadToEnd();
                ////XXX: Check if response is OK
                //resp.Close();

                XTM.WriteStartElement("purchase");
                XTM.WriteElementString("vhiId", mac);
                XTM.WriteStartElement("signedURL");
                XTM.WriteCData("http://drm.tvinci.com/GetLicense.aspx?vid=" + Request["vtiId"]);                
                XTM.WriteEndElement();
                XTM.WriteStartElement("status");
                XTM.WriteCData("OK");
                //}

                XTM.WriteEndDocument();
                break;
            case "category":                
                RedirectGWToChannel(Request.QueryString["chid"]);        
                break;
            case "channel":
                RedirectGWToChannel(sChID);   
                break;
            case "searchtitles":
                // XXX: fix this by channel ID (vsiId coming from the STB)
                string sSearch = Request["listId"];
                XTM.WriteStartElement("collections");
                XTM.WriteElementString("information", sSearch);
                XTM.WriteStartElement("items");

                dsItemInfo searchItemInfo = new APISearchLoader(wsUser, wsPass) { Name = sSearch, MediaType = 0, PageSize = 20, PictureSize = "0", OrderBy = TVPApi.OrderBy.ABC, IsPosterPic = false, WithInfo = true, GroupID = groupID, Platform = PlatformType.STB }.Execute();

                for (int isearch = 0; isearch < searchItemInfo.Item.Rows.Count; isearch++)
                {
                    XTM.WriteElementString("id", searchItemInfo.Item[isearch].ID + "-" + searchItemInfo.Item[isearch].MediaTypeID);
                }

                XTM.WriteEndDocument();
                break;
            default:
                XTM.WriteStartElement("service");
                XTM.WriteAttributeString("date", epoch.ToString());
                XTM.WriteStartElement("settings");
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "base");
                XTM.WriteCData("http://192.168.16.106/api");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "image");
                XTM.WriteCData("http://");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "photo");
                XTM.WriteCData("http://");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "content");
                XTM.WriteCData("http://192.168.16.106/api/gateways/netgem.aspx");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "purchase");
                XTM.WriteCData("http://192.168.16.106/api/gateways/netgem.aspx?type=purchase");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "purchaseStatus");
                XTM.WriteCData("http://192.168.16.106/api/gateways/netgem.aspx?type=purchasestatus");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "purchaseConfirmation");
                XTM.WriteCData("http://192.168.16.106/api/gateways/netgem.aspx?type=purchaseConfirmation");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "search-titles");
                XTM.WriteCData("http://192.168.16.106/api/gateways/netgem.aspx?type=searchtitles");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "search-peoples");
                XTM.WriteCData("http://192.168.16.106/api/searchPeoples.aspx");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "account");
                XTM.WriteCData("http://192.168.16.106/api/gateways/netgem.aspx?type=account");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "logStreamingStart");
                XTM.WriteCData("http://192.168.16.106/api/gateways/logStreamingStart.aspx");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "logDownloadEnd");
                XTM.WriteCData("http://192.168.16.106/tvpapi/gateways/logdownloadend.aspx");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "addCallCenterEvent");
                XTM.WriteCData("http://192.168.16.106/tvpapi/gateways/addCallCenterEvent.aspx");
                XTM.WriteEndElement();

                XTM.WriteEndDocument();
                break;
        }

        //XTM.Close();

        //m_dataCaching.AddData(HttpContext.Current.Request.Url.ToString().ToLower(), retVal, new string[] { }, 216000);

        Response.Clear();
        Response.Write(sw.ToString().Replace("utf-16", "utf-8"));
        Response.End();
    }

    private void RedirectGWToChannel(string chid)
    {
        eChannels pChid = (eChannels)Enum.Parse(typeof(eChannels), chid);
        switch (pChid)
        {
            case eChannels.Novebox:
                Server.Transfer(string.Concat("netgem_novebox.aspx", Request.Url.Query));
                break;
            case eChannels.Orange:
                Server.Transfer(string.Concat("netgem_orange.aspx", Request.Url.Query));
                break;
            case eChannels.Ipvision:
                Server.Transfer(string.Concat("netgem_ipvision.aspx", Request.Url.Query));
                break;
            default:
                break;
        }       
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
            case "ipvision":
                {
                    retVal = 125;
                    wsUser = "tvpapi_125";
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

    private string GetMediasWithSeperator(PermittedMediaContainer[] MyPermited)
    {
        StringBuilder sbRentedMedias = new StringBuilder();

        if (MyPermited != null)
        {
            foreach (PermittedMediaContainer MediaObj in MyPermited)
            {
                sbRentedMedias.Append(MediaObj.m_nMediaID.ToString() + "-");
            }
        }

        return sbRentedMedias.ToString();
    }
}
