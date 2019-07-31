//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using TVPApi;
//using System.Xml;
//using TVPApiServices;
//using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
//using TVPApiModule.Services;
//using System.Configuration;
//using TVPPro.SiteManager.Helper;
//using TVPPro.SiteManager.TvinciPlatform.Users;
//using TVPApiModule.DataLoaders;
//using TVPPro.SiteManager.DataEntities;
//using KLogMonitor;
//using System.Reflection;

///// <summary>
///// Summary description for GWFrontController
///// </summary>
//public class GWFrontController
//{
//    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

//    public struct ApiAccessInfo
//    {
//        public TVPApi.InitializationObject initObj { get; set; }
//        public int GroupID { get; set; }
//        public string DevSchema { get; set; }
//    }

//    private PageData pd;
//    private PageContext pc;
//    private XmlDocument xmlDoc;
//    private MediaService m_MediaService = new MediaService();
//    private SiteService m_SiteService = new SiteService();
//    private string identifier;
//    private PlatformType devType;
//    private ApiAccessInfo accessInfo;
//    private static int counter = 0;

//    public GWFrontController(ApiAccessInfo accessInfo, PlatformType devType)
//    {
//        this.devType = devType;
//        this.accessInfo = accessInfo;

//        pd = SiteMapManager.GetInstance.GetPageData(accessInfo.GroupID, devType);

//        int pageID = int.Parse(ConfigurationManager.AppSettings[string.Format("{0}_STBPageID", accessInfo.GroupID.ToString())]);
//        string lang = ConfigurationManager.AppSettings[string.Format("{0}_Lang", accessInfo.GroupID.ToString())];
//        pc = pd.GetPageByID(lang, pageID);

//        xmlDoc = new XmlDocument();
//        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
//        xmlDoc.AppendChild(xmlDec);
//    }

//    private XmlModels.GetServiceURLs.url createSettingsUrl(string type, string urlContent)
//    {
//        XmlModels.GetServiceURLs.url url = new XmlModels.GetServiceURLs.url();
//        url.type = type;
//        url.Value = urlContent;
//        return url;
//    }

//    public object GetServiceURLs(params object[] prms)
//    {
//        XmlModels.GetServiceURLs.service serv = new XmlModels.GetServiceURLs.service();

//        DateTime objUTC = DateTime.Now.ToUniversalTime();
//        long epoch = (objUTC.Ticks - 62135596800000000) / 10000;

//        serv.date = epoch.ToString();

//        string baseURL = ConfigurationManager.AppSettings["BaseNetGemURL"];
//        serv.settings.urlCollection = new XmlModels.GetServiceURLs.urlCollection();
//        serv.settings.urlCollection.Add(createSettingsUrl("base", baseURL + "/tvpapi"));
//        serv.settings.urlCollection.Add(createSettingsUrl("image", string.Empty));
//        serv.settings.urlCollection.Add(createSettingsUrl("photo", string.Empty));
//        serv.settings.urlCollection.Add(createSettingsUrl("content", baseURL + "/tvpapi/gateways/gateway.ashx?type=content&devtype=" + accessInfo.DevSchema));
//        serv.settings.urlCollection.Add(createSettingsUrl("purchase", baseURL + "/tvpapi/gateways/gateway.ashx?type=purchaseauth&devtype=" + accessInfo.DevSchema));
//        serv.settings.urlCollection.Add(createSettingsUrl("purchaseStatus", baseURL + "/tvpapi/gateways/gateway.ashx?type=purchaseprice&devtype=" + accessInfo.DevSchema));
//        serv.settings.urlCollection.Add(createSettingsUrl("purchaseConfirmation", baseURL + "/tvpapi/gateways/gateway.ashx?type=dopurchase&devtype=" + accessInfo.DevSchema));
//        serv.settings.urlCollection.Add(createSettingsUrl("search-people", baseURL + "/tvpapi/gateways/gateway.ashx?type=searchpeople.aspx&devtype=" + accessInfo.DevSchema));
//        serv.settings.urlCollection.Add(createSettingsUrl("search-titles", baseURL + "/tvpapi/gateways/gateway.ashx?type=searchtitles&devtype=" + accessInfo.DevSchema));
//        serv.settings.urlCollection.Add(createSettingsUrl("account", baseURL + "/tvpapi/gateways/gateway.ashx?type=accountInfo&devtype=" + accessInfo.DevSchema));
//        serv.settings.urlCollection.Add(createSettingsUrl("logStreamingStart", baseURL + "/tvpapi/gateways/logStreamingStart.aspx&devtype=" + accessInfo.DevSchema));
//        serv.settings.urlCollection.Add(createSettingsUrl("logStreamingEnd", baseURL + "/tvpapi/gateways/logdownloadend.aspx&devtype=" + accessInfo.DevSchema));
//        serv.settings.urlCollection.Add(createSettingsUrl("addCallCenterEvent", baseURL + "/tvpapi/gateways/addCallCenterEvent.aspx&devtype=" + accessInfo.DevSchema));
//        serv.settings.urlCollection.Add(createSettingsUrl("setlastposition", baseURL + "/tvpapi/gateways/gateway.ashx?type=hit&devtype=" + accessInfo.DevSchema));
//        serv.settings.urlCollection.Add(createSettingsUrl("getlastposition", baseURL + "/tvpapi/gateways/gateway.ashx?type=getlastposition&devtype=" + accessInfo.DevSchema));
//        serv.settings.urlCollection.Add(createSettingsUrl("logMedia", baseURL + "/tvpapi/gateways/gateway.ashx?type=mediamark&devtype=" + accessInfo.DevSchema));

//        return serv;
//    }

//    public object GetAccountInfo(params object[] prms)
//    {
//        //XXX: change this
//        XmlModels.GetAccountInfo.GetAccountInfo accountObj = new XmlModels.GetAccountInfo.GetAccountInfo();
//        XmlModels.GetAccountInfo.account acc = new XmlModels.GetAccountInfo.account();
//        XmlModels.GetAccountInfo.information info = new XmlModels.GetAccountInfo.information();
//        accountObj.accountCollection.Add(acc);

//        info.distributor = "TVinci";
//        info.contract = accessInfo.initObj.SiteGuid;
//        info.date = DateTime.UtcNow.ToShortDateString();
//        info.logo = "";
//        info.pay = "Credit Card";

//        UserResponseObject userResponseObject = m_SiteService.GetUserData(accessInfo.initObj, accessInfo.initObj.SiteGuid);
//        if (userResponseObject != null && userResponseObject.m_user != null && userResponseObject.m_user.m_oBasicData != null)
//            info.email = userResponseObject.m_user.m_oBasicData.m_sEmail;

//        acc.informationCollection.Add(info);

//        PermittedMediaContainer[] MediaPermitedItems = m_MediaService.GetUserPermittedItems(accessInfo.initObj);
//        if (MediaPermitedItems != null && MediaPermitedItems.Count() > 0)
//        {
//            MediaPermitedItems.OrderByDescending(x => x.m_dPurchaseDate).ToArray();
//            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(accessInfo.GroupID, accessInfo.initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
//            dsItemInfo ItemInfo = new APITVMRentalMultiMediaLoader(account.TVMUser, account.TVMPass, "full", 1)
//            {
//                GroupID = accessInfo.GroupID,
//                Platform = accessInfo.initObj.Platform,
//                MediasIdCotainer = MediaPermitedItems,
//                //SearchTokenSignature = GetMediasWithSeperator(MediaPermitedItems)
//            }.Execute(); // Type 1 means bring all types

//            XmlModels.GetAccountInfo.streams streams = new XmlModels.GetAccountInfo.streams();
//            acc.streamsCollection.Add(streams);
//            foreach (var item in ItemInfo.Item.OrderByDescending(x => x.PurchaseDate))
//            {
//                XmlModels.GetAccountInfo.stream s = new XmlModels.GetAccountInfo.stream();
//                streams.Add(s);

//                s.id = string.Concat(item.ID, "-", item.MediaTypeID); //XXX : fix!
//                s.title = item.Title;

//                int iFileID = int.Parse(item.FileID);

//                TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.MediaFileItemPricesContainer[] dictPrices = m_MediaService.GetItemPrices(accessInfo.initObj, new int[] { iFileID }, false);

//                MediaFileItemPricesContainer mediaPrice = null;
//                if (dictPrices != null)
//                {
//                    foreach (MediaFileItemPricesContainer mp in dictPrices)
//                    {
//                        if (mp.m_nMediaFileID == iFileID)
//                            mediaPrice = mp;
//                    }
//                }

//                if (mediaPrice != null && mediaPrice.m_oItemPrices != null && mediaPrice.m_oItemPrices.Length > 0)
//                {
//                    s.price = mediaPrice.m_oItemPrices[0].m_oFullPrice.m_dPrice.ToString();
//                    s.realprice = mediaPrice.m_oItemPrices[0].m_oPrice.m_dPrice.ToString();
//                }
//                else
//                {
//                    s.price = "0";
//                    s.realprice = "0";
//                }
//                s.date = item.PurchaseDate.ToShortDateString();
//            }
//        }

//        return accountObj;
//    }

//    public object GetAllChannels(params object[] prms)
//    {
//        XmlModels.GetAllChannels gac = new XmlModels.GetAllChannels();
//        List<XmlModels.GetAllChannelsChannel> channels = new List<XmlModels.GetAllChannelsChannel>();

//        foreach (PageGallery pg in pc.MainGalleries)
//        {
//            foreach (GalleryItem gi in pg.GalleryItems)
//            {
//                XmlModels.GetAllChannelsChannel newCh = new XmlModels.GetAllChannelsChannel();
//                newCh.Title = gi.Title;
//                newCh.Tvmch = gi.TVMChannelID.ToString();
//                newCh.Picsize = gi.PictureSize;

//                channels.Add(newCh);
//            }
//        }

//        gac.Items = channels.ToArray();
//        return gac;
//    }

//    public object GetChannelMedias(params object[] prms)
//    {
//        XmlModels.GetChannelMedias chMedias = new XmlModels.GetChannelMedias();

//        long mediaCount = 0;
//        string picSize = ConfigurationManager.AppSettings[string.Format("{0}_PicSize", accessInfo.GroupID.ToString())];
//        List<Media> lstMedias = m_MediaService.GetChannelMediaListWithMediaCount(accessInfo.initObj, (long)prms[0], picSize, 50, 0, ref mediaCount);        
        
//        if (lstMedias != null)
//        {
//            chMedias.Items = new XmlModels.GetMediaInfo[lstMedias.Count];
//            for (int i = 0; i < lstMedias.Count; i++)
//            {
//                XmlModels.GetMediaInfo info = GetMediaObj(lstMedias[i], (bool) prms[1]);
//                chMedias.Items[i] = info;
//            }
//        }

//        return chMedias;
//    }

//    public object GetChannelInfo(params object[] prms)
//    {
//        XmlModels.GetChannelInfo cInfo = new XmlModels.GetChannelInfo();

//        //TODO: Improve
//        foreach (PageGallery pg in pc.MainGalleries)
//        {
//            GalleryItem gallery = pg.GalleryItems.Where(x => x.TVMChannelID == (long)prms[0]).FirstOrDefault();

//            if (gallery == null)
//                continue;

//            cInfo.Title = gallery.Title;
//        }

//        return cInfo;
//    }

//    public object GetMediaInfo(params object[] prms)
//    {
//        string picSize = ConfigurationManager.AppSettings[string.Format("{0}_PicSize", accessInfo.GroupID.ToString())];

//        Media media = m_MediaService.GetMediaInfo(accessInfo.initObj, (long)prms[0], (int)prms[1], picSize, true);

//        //XXX Error handling
//        if (media == null)
//            return new XmlModels.GetMediaInfo();

//        return GetMediaObj(media, true);
//    }

//    private XmlModels.GetMediaInfo GetMediaObj(Media media, bool doFullMedia)
//    {
//        XmlModels.GetMediaInfo mInfo = new XmlModels.GetMediaInfo();
//        mInfo.Description = media.Description.Replace(@"<\p>", " ");
//        mInfo.MediaID = media.MediaID;
//        mInfo.FileID = media.FileID;
//        mInfo.MediaTypeID = media.MediaTypeID;
//        mInfo.Title = media.MediaName.Replace('(', ' ').Replace(')', ' ');

//        if (!doFullMedia)
//            return mInfo;

//        string runtime = (from meta in media.Metas where meta.Key.Equals("Display run time") select meta.Value).FirstOrDefault();
//        if (!string.IsNullOrEmpty(runtime))
//        {
//            string[] time = runtime.Split(new char[] { 'h', 'm' });
//            mInfo.Duration = string.Format("{0:0}", int.Parse(time[0]) * 60 + int.Parse(time[1]));
//        }
//        else
//            mInfo.Duration = "0";

//        mInfo.Rating = ((int)media.Rating).ToString();
//        mInfo.Copyright = (from meta in media.Metas where meta.Key.Equals("Production Name") select meta.Value).FirstOrDefault() ?? "None";
//        mInfo.Adult = "false";
//        mInfo.ProductionYear = (from meta in media.Metas where meta.Key.Equals("Release year") select meta.Value).FirstOrDefault() ?? "None";
//        mInfo.HD = "true";

//        string countries = (from tag in media.Tags where tag.Key.Equals("Country") select tag.Value).FirstOrDefault();
//        List<XmlModels.GetMediaInfoCountry> countryList = new List<XmlModels.GetMediaInfoCountry>();
//        if (countries != null)
//        {
//            foreach (string country in countries.Split('-'))
//                countryList.Add(new XmlModels.GetMediaInfoCountry() { Name = country });
//        }
//        mInfo.Country = countryList.ToArray();

//        string actors = (from tag in media.Tags where tag.Key.Equals("Cast") select tag.Value).FirstOrDefault();
//        List<XmlModels.GetMediaInfoActors> actorsList = new List<XmlModels.GetMediaInfoActors>();
//        if (actors != null)
//        {
//            foreach (string actor in actors.Split('|'))
//                actorsList.Add(new XmlModels.GetMediaInfoActors() { Name = actor });
//        }
//        mInfo.Actors = actorsList.ToArray();

//        string directors = (from tag in media.Tags where tag.Key.Equals("Director") select tag.Value).FirstOrDefault();
//        List<XmlModels.GetMediaInfoDirectors> directorsList = new List<XmlModels.GetMediaInfoDirectors>();
//        if (directors != null)
//        {
//            foreach (string director in directors.Split('-'))
//                directorsList.Add(new XmlModels.GetMediaInfoDirectors() { Name = director });
//        }
//        mInfo.Directors = directorsList.ToArray();

//        mInfo.URL = string.IsNullOrEmpty(media.URL) ? "http://drm.tvinci.com/movie_enc.wmv?rand=" + counter++ : media.URL;

//        // Prices
//        int fileId = int.Parse(media.FileID);
//        MediaFileItemPricesContainer[] dictPrices = m_MediaService.GetItemPrices(accessInfo.initObj, new int[] { fileId }, false);
//        MediaFileItemPricesContainer mediaPrice = null;
//        if (dictPrices != null)
//        {
//            foreach (MediaFileItemPricesContainer mp in dictPrices)
//            {
//                if (mp.m_nMediaFileID == fileId)
//                    mediaPrice = mp;
//            }
//        }

//        if (mediaPrice != null && mediaPrice.m_oItemPrices != null && mediaPrice.m_oItemPrices.Length > 0)
//        {
//            string sEndTime = string.Empty;

//            TVPPro.SiteManager.TvinciPlatform.Pricing.MediaFilePPVModule[] ppvmodules = new ApiPricingService(accessInfo.GroupID, accessInfo.initObj.Platform).GetPPVModuleListForMediaFiles(new int[] { fileId },
//                string.Empty, string.Empty, string.Empty);
//            if (ppvmodules != null && ppvmodules.Length > 0)
//                sEndTime = DateTime.Now.AddMinutes(ppvmodules[0].m_oPPVModules[0].m_oUsageModule.m_tsMaxUsageModuleLifeCycle).ToString("MM/dd/yyyy HH:mm:ss");

//            mInfo.LicenseDuration = (ppvmodules[0].m_oPPVModules[0].m_oUsageModule.m_tsMaxUsageModuleLifeCycle / 60).ToString();
//            mInfo.Price = mediaPrice.m_oItemPrices[0].m_oFullPrice.m_dPrice.ToString("0.00");
//            mInfo.EndDate = sEndTime;
//            mInfo.PPVModule = mediaPrice.m_oItemPrices[0].m_sPPVModuleCode;
//        }
//        else
//        {
//            mInfo.LicenseDuration = "0";
//            mInfo.Price = "0";
//            mInfo.PPVModule = "0";
//            mInfo.EndDate = "12/12/2030 00:00:00";
//        }
        
//        mInfo.TrailerURL = media.SubURL;
//        mInfo.PicURL = media.PicURL;

//        return mInfo;
//    }

//    public object PurchaseAuth(params object[] prms)
//    {
//        XmlModels.PurchaseAuthPurchase p = new XmlModels.PurchaseAuthPurchase();
//        p.challenge = "b252eb9a533c8ae37a462a267e1f2fa9";
//        p.state = "authentication";

//        return p;
//    }

//    public object PurchasePrice(params object[] prms)
//    {
//        XmlModels.PurchasePricePurchase p = new XmlModels.PurchasePricePurchase();

//        try
//        {
//            int stmpFileID = (int)prms[0];

//            TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.MediaFileItemPricesContainer[] dictPrices = m_MediaService.GetItemPrices(accessInfo.initObj, new int[] { stmpFileID }, false);

//            MediaFileItemPricesContainer mediaPrice = null;
//            if (dictPrices != null)
//            {
//                foreach (MediaFileItemPricesContainer mp in dictPrices)
//                {
//                    if (mp.m_nMediaFileID == stmpFileID)
//                        mediaPrice = mp;
//                }
//            }
//            if (mediaPrice.m_oItemPrices != null)
//                p.price = string.Format("{0:0.00}", mediaPrice.m_oItemPrices[0].m_oFullPrice.m_dPrice);
//            else
//                p.price = "0";

//            p.status = "OK";

//        }
//        catch (Exception ex)
//        {
//            logger.Error("Netgem purchasestatus Exception", ex);
//            //XXX: Check with documentation
//            p.status = "ERR";
//        }

//        return p;
//    }

//    public object DoPurchase(params object[] prms)
//    {
//        XmlModels.DoPurchasePurchase p = new XmlModels.DoPurchasePurchase();

//        int fileID = (int)prms[0];
//        string stmpPPVModule = ((int)prms[1]).ToString();
//        double iPrice = (double)prms[2];

//        //XXX: Do error checking
//        string response = new ApiConditionalAccessService(accessInfo.GroupID, accessInfo.initObj.Platform).DummyChargeUserForMediaFile(iPrice, "GBP", fileID,
//            stmpPPVModule, SiteHelper.GetClientIP(), accessInfo.initObj.SiteGuid, accessInfo.initObj.UDID);

//        p.signedURL = "http://drm.tvinci.com/GetLicense.aspx?vid=" + fileID;
//        p.status = "OK";
//        p.vhiId = accessInfo.initObj.UDID;

//        return p;
//    }

//    public object SearchTitles(params object[] prms)
//    {
//        XmlModels.SearchTitles.SearchTitles search = new XmlModels.SearchTitles.SearchTitles();

//        dsItemInfo searchItemInfo = new APISearchLoader(accessInfo.initObj.ApiUser, accessInfo.initObj.ApiPass)
//            {
//                Name = (string)prms[0],
//                MediaType = 0,
//                PageSize = 20,
//                PictureSize = "0",
//                OrderBy = TVPApi.OrderBy.ABC,
//                IsPosterPic = false,
//                WithInfo = true,
//                GroupID = accessInfo.GroupID,
//                Platform = accessInfo.initObj.Platform
//            }.Execute();

//        XmlModels.SearchTitles.collection coll = new XmlModels.SearchTitles.collection();
//        coll.information = (string)prms[0];
//        search.collectionCollection.Add(coll);

//        XmlModels.SearchTitles.items items = new XmlModels.SearchTitles.items();
//        coll.itemsCollection.Add(items);
//        for (int isearch = 0; isearch < searchItemInfo.Item.Rows.Count; isearch++)
//        {
//            XmlModels.SearchTitles.id newItem = new XmlModels.SearchTitles.id();
//            newItem.Value = searchItemInfo.Item[isearch].ID + "-" + searchItemInfo.Item[isearch].MediaTypeID;
//            items.Add(newItem);
//        }

//        return search;
//    }

//    public object DoHit(params object[] prms)
//    {
//        XmlModels.MediaMark.MediaMark mark = new XmlModels.MediaMark.MediaMark();

//        TVMAccountType account = SiteMapManager.GetInstance.GetPageData(accessInfo.GroupID, accessInfo.initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
//        string result = new APIMediaHit(account.TVMUser, account.TVMPass)
//        {
//            SiteGUID = accessInfo.initObj.SiteGuid,
//            DeviceUDID = accessInfo.initObj.UDID,
//            GroupID = accessInfo.GroupID,
//            Platform = accessInfo.initObj.Platform,
//            FileID = (long)prms[0],
//            MediaID = (long)prms[1],
//            Location = (int)prms[2]
//        }.Execute();

//        XmlModels.MediaMark.response res = new XmlModels.MediaMark.response();
//        res.type = "hit";
//        res.Value = result;
//        mark.Add(res);

//        return mark;
//    }

//    public object DoMediaMark(params object[] prms)
//    {
//        XmlModels.MediaMark.MediaMark mark = new XmlModels.MediaMark.MediaMark();

//        Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action eAction = (Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action)Enum.Parse(typeof(Tvinci.Data.TVMDataLoader.Protocols.MediaMark.action),
//            (string)prms[3]);

//        TVMAccountType account = SiteMapManager.GetInstance.GetPageData(accessInfo.GroupID, PlatformType.STB).GetTVMAccountByAccountType(AccountType.Regular);
//        string result = new APIMediaMark(account.TVMUser, account.TVMPass)
//        {
//            SiteGUID = accessInfo.initObj.SiteGuid,
//            Platform = accessInfo.initObj.Platform,
//            DeviceUDID = accessInfo.initObj.UDID,
//            GroupID = accessInfo.GroupID,
//            Action = eAction,
//            FileID = (long)prms[0],
//            MediaID = (long)prms[1],
//            Location = (int)prms[2]
//        }.Execute();

//        XmlModels.MediaMark.response res = new XmlModels.MediaMark.response();
//        res.type = "media_mark";
//        res.Value = result;
//        res.action = (string)prms[3];
//        mark.Add(res);

//        return mark;
//    }

//    public object GetLastPosition(params object[] prms)
//    {
//        XmlModels.MediaMark.MediaMark mark = new XmlModels.MediaMark.MediaMark();

//        TVMAccountType account = SiteMapManager.GetInstance.GetPageData(accessInfo.GroupID, accessInfo.initObj.Platform).GetTVMAccountByAccountType(AccountType.Regular);
//        TVPPro.SiteManager.Objects.MediaMarkObject mediaMarkObject = m_MediaService.GetMediaMark(accessInfo.initObj, (int)prms[0], null);

//        XmlModels.MediaMark.response res = new XmlModels.MediaMark.response();
//        res.type = "last_position";
//        res.Value = mediaMarkObject.nLocationSec.ToString();
//        mark.Add(res);

//        return mark;
//    }
//}