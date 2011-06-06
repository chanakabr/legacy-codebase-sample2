using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using Tvinci.Data.DataLoader;
using TVPApi;
using TVPApiModule.DataLoaders;
using TVPApiModule.Services;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

public partial class Gateways_NetGem_ipvision : BaseGateway
{
    static ILoaderCache m_dataCaching = LoaderCacheLite.Current;
    static int counter = 0;
    //XXX: Unify in one class
    public enum eChannels { Novebox = 50, Orange = 51, Ipvision = 901 };

    protected void Page_Load(object sender, EventArgs e)
    {
        Logger.Logger.Log("Netgem IPVision Request ", Request.Url.ToString(), "TVPApi");
        
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
        PlatformType platform = PlatformType.STB;

        broadcasterName = "ipvision";
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

        PageContext pc = pd.GetPageByID("en", 64); //

        DateTime objUTC = DateTime.Now.ToUniversalTime();
        long epoch = (objUTC.Ticks - 62135596800000000) / 10000;
        StringWriter sw = new StringWriter();
        XmlTextWriter XTM = new XmlTextWriter(sw);
        XTM.WriteStartDocument();

        switch (sType)
        {
            case "account":
                

                string sSiteGuid = string.Empty;//new UsersService(groupID, PlatformType.STB.ToString()).GetUserSiteGuid("adina@tvinci.com", "eliron27");

                XTM.WriteStartElement("account");
                XTM.WriteStartElement("information");
                XTM.WriteStartElement("distributor");
                XTM.WriteCData("");
                XTM.WriteEndElement(); // distributor
                XTM.WriteElementString("contract", sSiteGuid);
                XTM.WriteElementString("date", "05/09/2010 08:39");

                UserResponseObject userResponseObject = new ApiUsersService(groupID, platform).GetUserData(sSiteGuid);

                if (userResponseObject != null && userResponseObject.m_user != null && userResponseObject.m_user.m_oBasicData != null)
                {
                    XTM.WriteElementString("email", userResponseObject.m_user.m_oBasicData.m_sEmail);
                }
                XTM.WriteElementString("pay", "CreditCard");
                XTM.WriteElementString("logo", "");

                XTM.WriteEndElement(); // information

                PermittedMediaContainer[] MediaPermitedItems = new ApiConditionalAccessService(groupID, platform).GetUserPermittedItems(sSiteGuid);
                
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

                        Dictionary<int, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.MediaFileItemPricesContainer> dictPrices = new ApiConditionalAccessService(groupID, platform).GetItemsPrice(new int[] { iFileID }, sSiteGuid, true);

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

                string sMediaID = titId.Split('-')[0];
                string sMediaType = (titId.Contains('-')) ? titId.Split('-')[1] : "272";               

                Media media = tvpapiService.GetMediaInfo(GetInitObj(), wsUser, wsPass, long.Parse(sMediaID), int.Parse(sMediaType), "480X430", true);

                if (media != null)
                {
                    XTM.WriteStartElement("content");
                    //XTM.WriteElementString("synopsis", media.Description);
                    XTM.WriteStartElement("synopsis");
                    XTM.WriteCData(media.Description.Replace(@"<\p>", " "));
                    XTM.WriteEndElement();
                    XTM.WriteElementString("title", media.MediaName.Replace('(', ' ').Replace(')', ' '));
                    XTM.WriteElementString("durationInMinutes", string.Format("{0:0}", double.Parse(media.Duration) / 60));

                    XTM.WriteElementString("censorshipRating", string.Format("{0:0}", media.Rating));
                    XTM.WriteElementString("copyrightStudio", (from meta in media.Metas where meta.Key.Equals("Production Name") select meta.Value).FirstOrDefault());
                    XTM.WriteElementString("adult", "false");
                    XTM.WriteElementString("yearProd", (from meta in media.Metas where meta.Key.Equals("Release year") select meta.Value).FirstOrDefault());

                    XTM.WriteElementString("titID", media.MediaID + "-" + media.MediaTypeID + "-" + (int) eChannels.Ipvision);
                    XTM.WriteElementString("HD", "true"); //<---

                    XTM.WriteStartElement("nationalityNames");
                    string countires = (from tag in media.Tags where tag.Key.Equals("Country") select tag.Value).FirstOrDefault();
                    if (countires != null)
                    {
                        foreach (string country in countires.Split('-'))
                        {
                            XTM.WriteElementString("element", country);
                        }
                    }
                    XTM.WriteEndElement();
                    
                    XTM.WriteStartElement("actors");
                    string actors = (from tag in media.Tags where tag.Key.Equals("Cast") select tag.Value).FirstOrDefault();
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
                        foreach (string director in directors.Split('-'))
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

                    if (!String.IsNullOrEmpty(media.URL))
                        XTM.WriteElementString("URL", string.Concat(media.URL, "?bla=", counter++));
                    else
                        XTM.WriteElementString("URL", string.Concat("http://mystika.tvinci.cdngc.net/Ipvision/trailer.wmv", "?bla=", counter++));

                    //if (media.URL.StartsWith("http://"))                   
                    //    XTM.WriteElementString("URL", media.URL);
                    //else 
                    //    //XXX:  Change
                    //    XTM.WriteElementString("URL", media.URL); 
              
                    XTM.WriteElementString("durationInMinutes", string.Format("{0:0}", double.Parse(media.Duration) / 60));
                    XTM.WriteEndElement();

                    //XTM.WriteElementString("title", media.MediaName);

                    //if (TVPPro.SiteManager.Services.UsersService.Instance.GetUserID() == 0)
                    {
                        // new UsersServiceEx(groupID, PlatformType.STB.ToString().ToLower()).SignIn("idow@gmail.com", "eliron27");
                    }

                    int iFileId = int.Parse(media.FileID);
                    string tmpSiteGuid = new ApiUsersService(groupID, platform).SignIn("adina@tvinci.com", "eliron27");
                    Dictionary<int, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.MediaFileItemPricesContainer> dictPrice = new ApiConditionalAccessService(groupID, platform).GetItemsPrice(new int[] { iFileId }, tmpSiteGuid, true);
                    //Dictionary<int, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.MediaFileItemPricesContainer> dictPrice = null;

                    if (dictPrice != null && dictPrice.Keys.Contains(iFileId) && dictPrice[iFileId].m_oItemPrices != null && dictPrice[iFileId].m_oItemPrices.Length > 0)
                    {
                        //string response = new ConditionalAccessServiceEx(groupID, PlatformType.STB.ToString()).DummyChargeUserForMediaFileEx(dictPrice[iFileId].m_oItemPrices[0].m_oPrice.m_dPrice, dictPrice[iFileId].m_oItemPrices[0].m_oPrice.m_oCurrency.m_sCurrencyCD3, iFileId, dictPrice[iFileId].m_oItemPrices[0].m_sPPVModuleCode, SiteHelper.GetClientIP(), tmpSiteGuid);

                        string sEndTime = string.Empty;

                        TVPPro.SiteManager.TvinciPlatform.Pricing.MediaFilePPVModule[] ppvmodules = new ApiPricingService(groupID, platform).GetPPVModuleListForMediaFile(new int[] { iFileId }, string.Empty, string.Empty, string.Empty);
                        if (ppvmodules != null && ppvmodules.Length > 0)
                        {
                            sEndTime = DateTime.Now.AddMinutes(ppvmodules[0].m_oPPVModules[0].m_oUsageModule.m_tsMaxUsageModuleLifeCycle).ToString("dd/MM/yyyy HH:mm:ss");
                        }

                        XTM.WriteElementString("licenceDuration", (ppvmodules[0].m_oPPVModules[0].m_oUsageModule.m_tsMaxUsageModuleLifeCycle/60).ToString());
                        XTM.WriteElementString("licenceDurationUnit", "H");

                        XTM.WriteStartElement("price");
                        XTM.WriteElementString("price", string.Format("{0:0.00}", dictPrice[iFileId].m_oItemPrices[0].m_oFullPrice.m_dPrice));

                        XTM.WriteEndElement();
                        XTM.WriteElementString("endDate", sEndTime);
                        XTM.WriteElementString("vtiID", media.FileID + "-" + media.MediaTypeID + "-" + dictPrice[iFileId].m_oItemPrices[0].m_sPPVModuleCode + "-" + dictPrice[iFileId].m_oItemPrices[0].m_oPrice.m_dPrice + "-" + (int) eChannels.Ipvision);
                    }
                    else
                    {
                        XTM.WriteElementString("licenseDuration", "UL");
                        XTM.WriteElementString("licenseDurationUnit", "h");

                        XTM.WriteStartElement("price");
                        XTM.WriteElementString("price", "FREE");
                        XTM.WriteEndElement();

                        XTM.WriteElementString("vtiID", media.FileID + "-" + media.MediaTypeID + "-0-FREE-" + (int) eChannels.Ipvision);
                        XTM.WriteElementString("endDate", "06/06/2011 00:00:00");
                    }
                    XTM.WriteEndElement();

                    #endregion

                    XTM.WriteStartElement("trailerContentFileList");
                    XTM.WriteStartElement("element");
                    XTM.WriteElementString("URL", "http://mystika.tvinci.cdngc.net/Ipvision/trailer.wmv");
                    XTM.WriteElementString("encodingType", "");

                    XTM.WriteEndElement();
                    XTM.WriteEndElement();

                    XTM.WriteElementString("mediumImageRelativePath", media.PicURL.Replace("http://", string.Empty));
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
                XTM.WriteStartElement("purchase");
                try
                {
                    string vtiId = Request["vtiId"];
                    if (!string.IsNullOrEmpty(vtiId) && vtiId.Contains('-'))
                    {
                        string stmpFileID = vtiId.Split('-')[0];
                        string stmpMediaType = "0";// (vtiId.Contains('-')) ? vtiId.Split('-')[1] : "181";
                        string stmpPPVModule = vtiId.Split('-')[2];
                        double iPrice;
                        double.TryParse(vtiId.Split('-')[3], out iPrice);
                        string snewSiteGuid = new ApiUsersService(groupID, platform).SignIn("adina@tvinci.com", "eliron27");
                        //ConditionalAccessService.Instance.DummyChargeUserForMediaFile(iPrice, "USD", int.Parse(stmpFileID), stmpPPVModule, SiteHelper.GetClientIP());
                        string response = new ApiConditionalAccessService(groupID, platform).DummyChargeUserForMediaFile(iPrice, "GBP", int.Parse(stmpFileID), stmpPPVModule, SiteHelper.GetClientIP(), snewSiteGuid);
                        Logger.Logger.Log("Netgem purchasestatus", string.Format("Price:{0}, FileID:{1}, PPVModule:{2}, IP:{3}", iPrice, int.Parse(stmpFileID), stmpPPVModule, SiteHelper.GetClientIP()), "TVPApi");

                        XTM.WriteElementString("price", vtiId.Split('-')[3]);
                    }
                    else
                    {
                        XTM.WriteElementString("price", "3.00");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log("Netgem purchasestatus Exception ", ex.ToString() ,"TVPApi");
                }

                
                XTM.WriteStartElement("status");
                XTM.WriteCData("OK");                
                XTM.WriteEndElement();
                XTM.WriteEndDocument();
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
                string chid = Request.QueryString["chid"];
                string picsize = Request.QueryString["picsize"];
                string tvmChid = Request.QueryString["intChid"];

                XTM.WriteStartElement("collection");
                XTM.WriteStartElement("items");

                if (chid.Equals("myfavorites"))
                {
                    TVPPro.SiteManager.Services.UsersService.Instance.SignIn("idow@gmail.com", "eliron27");

                    Service service = new Service();
                    InitializationObject initObj = GetInitObj();
                    initObj.Locale = new Locale();
                    initObj.Locale.SiteGuid = TVPPro.SiteManager.Services.UsersService.Instance.GetUserID().ToString();
                    string userName = "tvpapi_93";
                    string pass = "11111";
                    UserItemType type = UserItemType.Favorite;
                    List<Media> userItems = service.GetUserItems(initObj, userName, pass, type, 0, "full", 20, 0);

                    foreach (Media fav in userItems)
                    {
                        XTM.WriteElementString("id", string.Concat(fav.MediaID, "-", fav.MediaTypeID + "-" + chid));
                    }
                }
                else
                {
                    long mediaCount = 0;
                    List<Media> lstMedias = tvpapiService.GetChannelMediaListWithMediaCount(GetInitObj(), wsUser, wsPass, long.Parse(tvmChid), "full", 50, 0, ref mediaCount);

                    if (lstMedias != null)
                    {
                        foreach (Media item in lstMedias)
                        {
                            XTM.WriteElementString("id", item.MediaID + "-" + item.MediaTypeID + "-" + chid);
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
                        XTM.WriteElementString("url", "/gateways/netgem.aspx?type=category&intChid=" + gi.TVMChannelID + "&picsize=" + gi.PictureSize + "&chid=" + sChID);
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


                dsItemInfo searchItemInfo = new APISearchLoader(wsUser, wsPass) { Name = sSearch, MediaType = 0, PageSize = 20, PictureSize = "0", OrderBy = TVPApi.OrderBy.ABC, IsPosterPic = false, WithInfo = true, GroupID=groupID, Platform = PlatformType.STB }.Execute();

                for (int isearch = 0; isearch < searchItemInfo.Item.Rows.Count; isearch++)
                {
                    XTM.WriteElementString("id", searchItemInfo.Item[isearch].ID + "-" + searchItemInfo.Item[isearch].MediaTypeID);
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
                /*
                XTM.WriteStartElement("service");
                XTM.WriteAttributeString("date", epoch.ToString());
                XTM.WriteStartElement("settings");
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "base");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi");
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
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi/gateways/logdownloadend.aspx");
                XTM.WriteEndElement();
                XTM.WriteStartElement("url");
                XTM.WriteAttributeString("type", "addCallCenterEvent");
                XTM.WriteCData("http://platform-us.tvinci.com/tvpapi/gateways/addCallCenterEvent.aspx");
                XTM.WriteEndElement();

                XTM.WriteEndDocument();*/
                break;
        }

        //XTM.Close();

        //m_dataCaching.AddData(HttpContext.Current.Request.Url.ToString().ToLower(), retVal, new string[] { }, 216000);

        Response.Clear();
        Response.Write(sw.ToString().Replace("utf-16", "utf-8"));
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
            case "ipvision":
                {
                    retVal = 125;
                    wsUser = "tvpapi_126";
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
        return "tvp_125";
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