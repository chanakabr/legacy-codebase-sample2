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
using TVPApiServices;

public partial class Gateways_NetGem : BaseGateway
{
    static ILoaderCache m_dataCaching = LoaderCacheLite.Current;
    private MediaService m_mediaService = new MediaService();

    protected void Page_Load(object sender, EventArgs e)
    {
        //TODO: Logger.Logger.Log("Netgem Request ", Request.Url.ToString(), "TVPApi");
        
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

        string sType = Request.QueryString["type"];
        string titId = Request.QueryString["titId"];
        if (!string.IsNullOrEmpty(titId))
        {
            sType = "content";
        }

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

                TVPPro.SiteManager.Services.UsersService.Instance.SignIn("idow@gmail.com", "eliron27");

                long iSiteGuid = TVPPro.SiteManager.Services.UsersService.Instance.GetUserID();

                XTM.WriteStartElement("account");
                XTM.WriteStartElement("information");
                XTM.WriteStartElement("distributor");
                XTM.WriteCData("");
                XTM.WriteEndElement(); // distributor
                XTM.WriteElementString("contract", iSiteGuid.ToString());
                XTM.WriteElementString("date", "05/09/2010 08:39");
                XTM.WriteElementString("pay", "CreditCard");
                XTM.WriteElementString("email", TVPPro.SiteManager.Services.UsersService.Instance.GetUserData(iSiteGuid.ToString()).m_user.m_oBasicData.m_sEmail);
                XTM.WriteElementString("logo", "");

                XTM.WriteEndElement(); // information

                PermittedMediaContainer[] MediaPermitedItems = ConditionalAccessService.Instance.GetUserPermittedItems();

                XTM.WriteStartElement("streams");
                if (MediaPermitedItems != null && MediaPermitedItems.Count() > 0)
                {
                    dsItemInfo ItemInfo = new TVMRentalMultiMediaLoader(wsUser, wsPass, "0", 1) { MediasIdCotainer = MediaPermitedItems, SearchTokenSignature = GetMediasWithSeperator(MediaPermitedItems) }.Execute(); // Type 1 means bring all types

                    for (int i = 0; i < ItemInfo.Item.Rows.Count; i++)
                    {
                        XTM.WriteStartElement("stream");
                        XTM.WriteAttributeString("id", string.Concat(ItemInfo.Item[i].ID, "|", ItemInfo.Item[i].MediaTypeID));
                        XTM.WriteStartElement("title");
                        XTM.WriteCData(ItemInfo.Item[i].Title);
                        XTM.WriteEndElement();//title

                        int iFileID = int.Parse(ItemInfo.Item[i].FileID);
                        Dictionary<int, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.MediaFileItemPricesContainer> dictPrices = TVPPro.SiteManager.Services.ConditionalAccessService.Instance.GetItemsPrice(new int[] { iFileID }, true);

                        if (dictPrices != null && dictPrices.Keys.Contains(iFileID) && dictPrices[iFileID].m_oItemPrices != null && dictPrices[iFileID].m_oItemPrices.Length > 0)
                        {
                            XTM.WriteElementString("price", dictPrices[iFileID].m_oItemPrices[0].m_oFullPrice.m_dPrice.ToString());
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
                string sMediaID = titId.Split('|')[0];
                string sMediaType = (titId.Contains('|')) ? titId.Split('|')[1] : "181";

                Media media = m_mediaService.GetMediaInfo(GetInitObj(), wsUser, wsPass, long.Parse(sMediaID), int.Parse(sMediaType), "345X480", true);

                if (media != null)
                {
                    XTM.WriteStartElement("content");
                    XTM.WriteElementString("synopsis", media.Description);
                    XTM.WriteElementString("title", media.MediaName);
                    //XTM.WriteElementString("durationInMinutes", string.Format("{0:0}", double.Parse(media.Duration) / 60));

                    XTM.WriteElementString("censorshipRating", string.Format("{0:0}", media.Rating));
                    XTM.WriteElementString("copyrightStudio", (from meta in media.Metas where meta.Key.Equals("Production Name") select meta.Value).FirstOrDefault());
                    XTM.WriteElementString("adult", "false");
                    XTM.WriteElementString("yearProd", (from meta in media.Metas where meta.Key.Equals("Production Year") select meta.Value).FirstOrDefault());

                    XTM.WriteElementString("titID", media.MediaID + "|" + media.MediaTypeID);
                    XTM.WriteElementString("HD", "true"); //<---

                    XTM.WriteStartElement("nationalityNames");
                    string countires = (from tag in media.Tags where tag.Key.Equals("Country") select tag.Value).FirstOrDefault();
                    if (countires != null)
                    {
                        foreach (string country in countires.Split('|'))
                        {
                            XTM.WriteElementString("element", country);
                        }
                    }
                    XTM.WriteEndElement();

                    XTM.WriteStartElement("actors");
                    string actors = (from tag in media.Tags where tag.Key.Equals("starring") select tag.Value).FirstOrDefault();
                    if (actors != null)
                    {
                        foreach (string actor in actors.Split('|'))
                        {
                            XTM.WriteStartElement("xml-link");
                            XTM.WriteElementString("title", actor);
                            XTM.WriteElementString("url", "");
                            XTM.WriteEndElement();
                        }
                    }
                    XTM.WriteEndElement();

                    XTM.WriteStartElement("directors");
                    string directors = (from tag in media.Tags where tag.Key.Equals("Director") select tag.Value).FirstOrDefault();
                    if (directors != null)
                    {
                        foreach (string director in directors.Split('|'))
                        {
                            XTM.WriteStartElement("xml-link");
                            XTM.WriteElementString("title", director);
                            XTM.WriteElementString("url", "");
                            XTM.WriteEndElement();
                        }
                    }
                    XTM.WriteEndElement();

                    #region DTRProduct
                    XTM.WriteStartElement("DTRProduct");
                    XTM.WriteStartElement("contentFile");
                    XTM.WriteElementString("URL", "http://content1.catalog.video.msn.com/e2/ds/es-xl/ESXL_Novebox/ESXL_Novebox_PlanV_capitulos/ce70caa9-1d1a-4f7b-93da-3694f8922c2f.wmv");
                    XTM.WriteElementString("durationInMinutes", "100");//double.Parse(media.Duration) / 60));
                    XTM.WriteEndElement();

                    //XTM.WriteElementString("title", media.MediaName);

                    if (TVPPro.SiteManager.Services.UsersService.Instance.GetUserID() == 0)
                    {
                        TVPPro.SiteManager.Services.UsersService.Instance.SignIn("idow@gmail.com", "eliron27");
                    }

                    int iFileId = int.Parse(media.FileID);
                    Dictionary<int, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.MediaFileItemPricesContainer> dictPrice = TVPPro.SiteManager.Services.ConditionalAccessService.Instance.GetItemsPrice(new int[] { iFileId }, false);

                    if (dictPrice != null && dictPrice.Keys.Contains(iFileId) && dictPrice[iFileId].m_oItemPrices != null && dictPrice[iFileId].m_oItemPrices.Length > 0)
                    {
                        XTM.WriteElementString("licenceDuration", dictPrice[iFileId].m_oItemPrices[0].m_oPPVDescription[0].m_sValue);
                        XTM.WriteElementString("licenceDurationUnit", ".");

                        XTM.WriteStartElement("price");
                        XTM.WriteElementString("price", string.Format("{0:0.00}", dictPrice[iFileId].m_oItemPrices[0].m_oPrice.m_dPrice));
                        XTM.WriteEndElement();

                        XTM.WriteElementString("vtiID", media.FileID + "|" + media.MediaTypeID + "|" + dictPrice[iFileId].m_oItemPrices[0].m_sPPVModuleCode + "|" + dictPrice[iFileId].m_oItemPrices[0].m_oPrice.m_dPrice);
                    }
                    else
                    {
                        XTM.WriteElementString("licenceDuration", "U");
                        XTM.WriteElementString("licenceDurationUnit", "L");

                        XTM.WriteStartElement("price");
                        XTM.WriteElementString("price", "FREE ");
                        XTM.WriteEndElement();

                        XTM.WriteElementString("endDate", "01/01/2012 00:00:00");
                    }



                    XTM.WriteEndElement();

                    #endregion

                    XTM.WriteStartElement("trailerContentFileList");
                    XTM.WriteStartElement("element");
                    XTM.WriteElementString("URL", "http://content1.catalog.video.msn.com/e2/ds/es-xl/ESXL_Novebox/ESXL_Novebox_PlanV_capitulos/ce70caa9-1d1a-4f7b-93da-3694f8922c2f.wmv");
                    XTM.WriteElementString("encodingType", "");

                    XTM.WriteEndElement();
                    XTM.WriteEndElement();

                    XTM.WriteElementString("mediumImageRelativePath", media.PicURL.Replace("http://tvinci.panthercustomer.com", string.Empty));
                    XTM.WriteElementString("mediumImageAbsolutePath", media.PicURL);
                    XTM.WriteEndDocument();
                }

                break;
            case "purchase":
                XTM.WriteStartElement("purchase");
                XTM.WriteElementString("state", "authentication");
                XTM.WriteElementString("challenge", "b252eb9a533c8ae37a462a267e1f2fa9");
                XTM.WriteEndElement();
                XTM.WriteEndDocument();
                break;
            case "purchasestatus":
                try
                {
                    string vtiId = Request["vtiId"];
                    if (!string.IsNullOrEmpty(vtiId) && vtiId.Contains('|'))
                    {
                        string stmpFileID = vtiId.Split('|')[0];
                        string stmpMediaType = (vtiId.Contains('|')) ? vtiId.Split('|')[1] : "181";
                        string stmpPPVModule = vtiId.Split('|')[2];
                        double iPrice;
                        double.TryParse(vtiId.Split('|')[3], out iPrice);
                        ConditionalAccessService.Instance.DummyChargeUserForMediaFile(iPrice, "USD", int.Parse(stmpFileID), stmpPPVModule, SiteHelper.GetClientIP());
                        //TODO: Logger.Logger.Log("Netgem purchasestatus", string.Format("Price:{0}, FileID:{1}, PPVModule:{2}, IP:{3}", iPrice, int.Parse(stmpFileID), stmpPPVModule, SiteHelper.GetClientIP()), "TVPApi");
                    }
                }
                catch (Exception ex)
                {
                    //TODO: Logger.Logger.Log("Netgem purchasestatus Exception ", ex.ToString(), "TVPApi");
                }

                XTM.WriteStartElement("purchase");
                XTM.WriteElementString("price", "2.99");
                XTM.WriteStartElement("status");
                XTM.WriteCData("OK");
                XTM.WriteEndElement();
                XTM.WriteEndElement();
                XTM.WriteEndDocument();
                break;
            case "purchaseConfirmation":
                string hmac = Request.QueryString["hmac"];
                string mac = Request.QueryString["identity"];
                HttpWebRequest HttpWReq = (HttpWebRequest)WebRequest.Create(string.Format(@"http://request-dms.netboxtv.netgem.com/challenge/{0}/b252eb9a533c8ae37a462a267e1f2fa9/{1}", mac, hmac));
                HttpWReq.Method = "POST";
                HttpWReq.ContentType = "application/x-www-form-urlencoded";

                string xmlPost = string.Format("<data><user>{0}</user><challenge>b252eb9a533c8ae37a462a267e1f2fa9</challenge><hmac>{1}</hmac></data>", mac, hmac);
                byte[] byteArray = Encoding.UTF8.GetBytes(xmlPost);
                HttpWReq.ContentLength = byteArray.Length;

                using (Stream stream = HttpWReq.GetRequestStream())
                {
                    stream.Write(byteArray, 0, byteArray.Length);
                    stream.Close();

                    HttpWebResponse resp = (HttpWebResponse)HttpWReq.GetResponse();
                    StreamReader responseReader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
                    string res = responseReader.ReadToEnd();
                    resp.Close();

                    XTM.WriteStartElement("purchase");
                    XTM.WriteElementString("vhiId", mac);
                    XTM.WriteElementString("price", "0.99");
                    XTM.WriteStartElement("signedURL");
                    XTM.WriteCData("http://www.dev.glowria.fr/ws/license/deliver.do?token=ZLzFbHnArjD%2FYb2y6Yoz57sVFTzHxRrw0UFCfgnGFSFc9HCX2qvdKQ%3D%3D");
                    XTM.WriteEndElement();
                    XTM.WriteElementString("challenge", "b252eb9a533c8ae37a462a267e1f2fa9");
                    XTM.WriteStartElement("url");
                    XTM.WriteCData("");
                    XTM.WriteEndElement();

                    XTM.WriteStartElement("status");
                    XTM.WriteCData("OK");

                    XTM.WriteEndElement();
                    XTM.WriteElementString("reattrib", "false");
                    XTM.WriteEndElement();

                }
                XTM.WriteEndDocument();
                break;
            case "category":
                string chid = Request.QueryString["chid"];
                string picsize = Request.QueryString["picsize"];

                XTM.WriteStartElement("collection");
                XTM.WriteStartElement("items");

                if (chid.Equals("myfavorites"))
                {
                    TVPPro.SiteManager.Services.UsersService.Instance.SignIn("chen@tvinci.com", "eliron27");

                    InitializationObject initObj = GetInitObj();
                    initObj.Locale = new Locale();
                    initObj.Locale.SiteGuid = TVPPro.SiteManager.Services.UsersService.Instance.GetUserID().ToString();
                    string userName = "tvpapi_93";
                    string pass = "11111";
                    UserItemType type = UserItemType.Favorite;
                    List<Media> userItems = m_mediaService.GetUserItems(initObj, userName, pass, type, 0, "full", 20, 0);

                    foreach (Media fav in userItems)
                    {
                        XTM.WriteElementString("id", string.Concat(fav.MediaID, "|", fav.MediaTypeID));
                    }
                }
                else
                {
                    long mediaCount = 0;
                    List<Media> lstMedias = m_mediaService.GetChannelMediaListWithMediaCount(GetInitObj(), wsUser, wsPass, long.Parse(chid), "full", 50, 0, ref mediaCount);

                    if (lstMedias != null)
                    {
                        foreach (Media item in lstMedias)
                        {
                            XTM.WriteElementString("id", item.MediaID + "|" + item.MediaTypeID);
                        }
                    }
                }

                XTM.WriteEndDocument();
                break;
            case "channel":

                XTM.WriteStartElement("collections");
                XTM.WriteAttributeString("adult", "false");

                //XTM.WriteStartElement("xml-link");
                //XTM.WriteElementString("name", "My Favorites");
                //XTM.WriteElementString("url", "/gateways/netgem.aspx?type=category&chid=myfavorites&picsize=345X480");
                //XTM.WriteEndElement();

                foreach (PageGallery pg in pc.MainGalleries)
                {
                    foreach (GalleryItem gi in pg.GalleryItems)
                    {
                        XTM.WriteStartElement("xml-link");
                        XTM.WriteElementString("name", gi.Title);
                        XTM.WriteElementString("url", "/gateways/netgem.aspx?type=category&chid=" + gi.TVMChannelID + "&picsize=" + gi.PictureSize);
                        XTM.WriteEndElement();
                    }
                }

                XTM.WriteEndElement();

                XTM.WriteEndDocument();
                break;
            case "searchtitles":
                string sSearch = Request["listId"];
                XTM.WriteStartElement("collections");
                XTM.WriteElementString("information", sSearch);
                XTM.WriteStartElement("items");


                dsItemInfo searchItemInfo = new SearchMediaLoader(wsUser, wsPass) { Name = sSearch, MediaType = 181, PageSize = 20, PictureSize = "345X480", OrderBy = Enums.eOrderBy.ABC, IsPosterPic = false, WithInfo = true }.Execute();

                for (int isearch = 0; isearch < searchItemInfo.Item.Rows.Count; isearch++)
                {
                    XTM.WriteElementString("id", searchItemInfo.Item[isearch].ID + "|181");
                }

                XTM.WriteEndDocument();//items
                //XTM.WriteEndDocument();//collections
                //                <collection>
                //<information>Search pattern</information>
                //<items>
                //<id>Stream ID 0</id>
                //....
                //</items>
                //</collection>
                break;
            default:
                XTM.WriteStartElement("service");
                XTM.WriteAttributeString("date", epoch.ToString());
                XTM.WriteStartElement("settings");
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "base");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "image");
                XTM.WriteCData("http://tvinci.panthercustomer.com");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "photo");
                XTM.WriteCData("http://tvinci.panthercustomer.com");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "content");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi/gateways/netgem.aspx");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "purchase");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi/gateways/netgem.aspx?type=purchase");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "purchaseStatus");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi/gateways/netgem.aspx?type=purchasestatus");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "purchaseConfirmation");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi/gateways/netgem.aspx?type=purchaseConfirmation");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "search-titles");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi/gateways/netgem.aspx?type=searchtitles");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "search-peoples");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi/searchPeoples.aspx");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "account");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi/gateways/netgem.aspx?type=account");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "logStreamingStart");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi/gateways/logStreamingStart.aspx");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "logDownloadEnd");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi/gateways/logStreamingStart.aspx");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "addCallCenterEvent");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi/gateways/addCallCenterEvent.aspx");
                XTM.WriteEndElement();

                XTM.WriteEndDocument();
                break;
        }

        //XTM.Close();

        //m_dataCaching.AddData(HttpContext.Current.Request.Url.ToString().ToLower(), retVal, new string[] { }, 216000);

        Response.Clear();
        Response.Write(sw.ToString().Replace("utf-16", "utf-8"));
        Response.Expires = -1;
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

    private string GetMediasWithSeperator(PermittedMediaContainer[] MyPermited)
    {
        StringBuilder sbRentedMedias = new StringBuilder();

        if (MyPermited != null)
        {
            foreach (PermittedMediaContainer MediaObj in MyPermited)
            {
                sbRentedMedias.Append(MediaObj.m_nMediaID.ToString() + "|");
            }
        }

        return sbRentedMedias.ToString();
    }
}
