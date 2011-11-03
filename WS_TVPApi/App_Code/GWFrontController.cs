using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using System.Xml;
using TVPApiServices;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPApiModule.Services;
using System.Configuration;

/// <summary>
/// Summary description for GWFrontController
/// </summary>
public class GWFrontController
{
    public struct ApiAccessInfo
    {
        public TVPApi.InitializationObject initObj { get; set; }
        public int GroupID { get; set; }
    }

    private PageData pd;
    private PageContext pc;
    private XmlDocument xmlDoc;
    private MediaService m_MediaService = new MediaService();
    private SiteService m_SiteService = new SiteService();
    private string identifier;
    private PlatformType devType;
    private ApiAccessInfo accessInfo;

    public GWFrontController(ApiAccessInfo accessInfo, string identifier, PlatformType devType)
    {
        this.identifier = identifier;
        this.devType = devType;
        this.accessInfo = accessInfo;

        pd = SiteMapManager.GetInstance.GetPageData(accessInfo.GroupID, devType);        
        pc = accessInfo.GroupID == 93 ? pd.GetPageByID("es", 69) : pd.GetPageByID("en", 64);

        xmlDoc = new XmlDocument();
        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(xmlDec);
    }

    private XmlModels.GetServiceURLs.url createSettingsUrl(string type, string urlContent)
    {        
        XmlModels.GetServiceURLs.url url = new XmlModels.GetServiceURLs.url();
        url.type = type;
        url.Value = urlContent;
        return url;
    }

    public object GetServiceURLs(params object[] prms)
    {
        XmlModels.GetServiceURLs.service serv = new XmlModels.GetServiceURLs.service();

        DateTime objUTC = DateTime.Now.ToUniversalTime();
        long epoch = (objUTC.Ticks - 62135596800000000) / 10000;

        serv.date = epoch.ToString();

        string baseURL = ConfigurationManager.AppSettings["BaseNetGemURL"];
        serv.settings.urlCollection = new XmlModels.GetServiceURLs.urlCollection();
        serv.settings.urlCollection.Add(createSettingsUrl("base", baseURL + "/tvpapi"));                
        serv.settings.urlCollection.Add(createSettingsUrl("image", string.Empty));
        serv.settings.urlCollection.Add(createSettingsUrl("photo", string.Empty));
        serv.settings.urlCollection.Add(createSettingsUrl("content", baseURL + "/tvpapi/gateways/gateway.ashx?type=content"));
        serv.settings.urlCollection.Add(createSettingsUrl("purchase", baseURL + "/tvpapi/gateways/gateway.ashx?type=purchase"));
        serv.settings.urlCollection.Add(createSettingsUrl("purchaseStatus", baseURL + "/tvpapi/gateways/gateway.ashx?type=purchasestatus"));
        serv.settings.urlCollection.Add(createSettingsUrl("purchaseConfirmation", baseURL + "/tvpapi/gateways/gateway.ashx?type=purchaseConfirmation"));
        serv.settings.urlCollection.Add(createSettingsUrl("search-people", baseURL + "/tvpapi/gateways/searchPeoples.aspx"));
        serv.settings.urlCollection.Add(createSettingsUrl("search-titles", baseURL + "/tvpapi/gateways/gateway.ashx?type=searchtitles"));
        serv.settings.urlCollection.Add(createSettingsUrl("account", baseURL + "/tvpapi/gateways/gateway.ashx?type=account"));
        serv.settings.urlCollection.Add(createSettingsUrl("logStreamingStart", baseURL + "/tvpapi/gateways/logStreamingStart.aspx"));
        serv.settings.urlCollection.Add(createSettingsUrl("logStreamingEnd", baseURL + "/tvpapi/gateways/logdownloadend.aspx"));
        serv.settings.urlCollection.Add(createSettingsUrl("addCallCenterEvent", baseURL + "/tvpapi/gateways/addCallCenterEvent.aspx"));
        serv.settings.urlCollection.Add(createSettingsUrl("setlastposition", baseURL + "/tvpapi/gateways/gateway.ashx?type=hit"));
        serv.settings.urlCollection.Add(createSettingsUrl("getlastposition", baseURL + "/tvpapi/gateways/gateway.ashx?type=getlastposition"));
        serv.settings.urlCollection.Add(createSettingsUrl("logMedia", baseURL + "/tvpapi/gateways/gateway.ashx?type=mediamark"));
        
        return serv;
    }

    public object GetAllChannels(params object[] prms)
    {
        XmlModels.GetAllChannels gac = new XmlModels.GetAllChannels();
        List<XmlModels.GetAllChannelsChannel> channels = new List<XmlModels.GetAllChannelsChannel>();
        
        foreach (PageGallery pg in pc.MainGalleries)
        {
            foreach (GalleryItem gi in pg.GalleryItems)
            {
                XmlModels.GetAllChannelsChannel newCh = new XmlModels.GetAllChannelsChannel();
                newCh.Title = gi.Title;
                newCh.Tvmch = gi.TVMChannelID.ToString();
                newCh.Picsize = gi.PictureSize;

                channels.Add(newCh);
            }
        }

        gac.Items = channels.ToArray();
        return gac;
    }

    public object GetChannelMedias(params object[] prms)
    {
        XmlModels.GetChannelMedias chMedias = new XmlModels.GetChannelMedias();
        
        long mediaCount = 0;
        List<Media> lstMedias = m_MediaService.GetChannelMediaListWithMediaCount(accessInfo.initObj, (long)prms[0], "full", 50, 0, ref mediaCount);
        List<XmlModels.GetChannelMediasMedia> allMedias = new List<XmlModels.GetChannelMediasMedia>();

        if (lstMedias != null)
        {
            foreach (Media item in lstMedias)
            {
                XmlModels.GetChannelMediasMedia medias = new XmlModels.GetChannelMediasMedia();
                medias.ID = item.MediaID;
                medias.Type = item.MediaTypeID;                
                allMedias.Add(medias);
            }
        }

        chMedias.Items = allMedias.ToArray();
        return chMedias;
    }

    public object GetChannelInfo(params object[] prms)
    {
        XmlModels.GetChannelInfo cInfo = new XmlModels.GetChannelInfo();
                
        //TODO: Improve
        foreach (PageGallery pg in pc.MainGalleries)
        {
            GalleryItem gallery = pg.GalleryItems.Where(x=> x.TVMChannelID == (long) prms[0]).FirstOrDefault();
           
            if (gallery == null)
                continue;

            cInfo.Title = gallery.Title;
        }

        return cInfo;
    }

    public object GetMediaInfo(params object[] prms)
    {
        XmlModels.GetMediaInfo mInfo = new XmlModels.GetMediaInfo();

        Media media = m_MediaService.GetMediaInfo(accessInfo.initObj, (long)prms[0], (int)prms[1], "480X430", true);

        //XXX Error handling
        if (media == null)
            return mInfo;

        mInfo.Description = media.Description.Replace(@"<\p>", " ");        
        mInfo.MediaID = media.MediaID;        
        mInfo.Title = media.MediaName.Replace('(', ' ').Replace(')', ' ');
        
        string runtime = (from meta in media.Metas where meta.Key.Equals("Display run time") select meta.Value).FirstOrDefault();        
        if (!string.IsNullOrEmpty(runtime))
        {
            string[] time = runtime.Split(new char[] { 'h', 'm' });            
            mInfo.Duration = string.Format("{0:0}", int.Parse(time[0]) * 60 + int.Parse(time[1]));
        }
        else
            mInfo.Duration = "0";
        
        mInfo.Rating = media.Rating.ToString();
        mInfo.Copyright = (from meta in media.Metas where meta.Key.Equals("Production Name") select meta.Value).FirstOrDefault() ?? "None";
        mInfo.Adult = "false";        
        mInfo.ProductionYear = (from meta in media.Metas where meta.Key.Equals("Release year") select meta.Value).FirstOrDefault() ?? "None";        
        mInfo.HD = "true";
        
        string countries = (from tag in media.Tags where tag.Key.Equals("Country") select tag.Value).FirstOrDefault();
        List<XmlModels.GetMediaInfoCountry> countryList = new List<XmlModels.GetMediaInfoCountry>();
        if (countries != null)
        {
            foreach (string country in countries.Split('-'))
                countryList.Add(new XmlModels.GetMediaInfoCountry() { Name = country });
        }
        mInfo.Country = countryList.ToArray();
        
        string actors = (from tag in media.Tags where tag.Key.Equals("Cast") select tag.Value).FirstOrDefault();
        List<XmlModels.GetMediaInfoActors> actorsList = new List<XmlModels.GetMediaInfoActors>();
        if (actors != null)
        {
            foreach (string actor in actors.Split('|'))
                countryList.Add(new XmlModels.GetMediaInfoCountry() { Name = actor });
        }
        mInfo.Actors = actorsList.ToArray();

        string directors = (from tag in media.Tags where tag.Key.Equals("Director") select tag.Value).FirstOrDefault();
        List<XmlModels.GetMediaInfoDirectors> directorsList = new List<XmlModels.GetMediaInfoDirectors>();
        if (directors != null)
        {
            foreach (string director in directors.Split('-'))
                directorsList.Add(new XmlModels.GetMediaInfoDirectors() { Name = director });        
        }
        mInfo.Directors = directorsList.ToArray();
        
        mInfo.URL = media.URL;

        // Prices
        int fileId = int.Parse(media.FileID);
        MediaFileItemPricesContainer[] dictPrices = m_MediaService.GetItemPrices(accessInfo.initObj, new int[] { fileId }, false);
        MediaFileItemPricesContainer mediaPrice = null;
        if (dictPrices != null)
        {
            foreach (MediaFileItemPricesContainer mp in dictPrices)
            {
                if (mp.m_nMediaFileID == fileId)
                    mediaPrice = mp;
            }
        }

        if (mediaPrice != null && mediaPrice.m_oItemPrices != null && mediaPrice.m_oItemPrices.Length > 0)
        {
            string sEndTime = string.Empty;

            TVPPro.SiteManager.TvinciPlatform.Pricing.MediaFilePPVModule[] ppvmodules = new ApiPricingService(accessInfo.GroupID, accessInfo.initObj.Platform).GetPPVModuleListForMediaFiles(new int[] { fileId },
                string.Empty, string.Empty, string.Empty);
            if (ppvmodules != null && ppvmodules.Length > 0)
                sEndTime = DateTime.Now.AddMinutes(ppvmodules[0].m_oPPVModules[0].m_oUsageModule.m_tsMaxUsageModuleLifeCycle).ToString("MM/dd/yyyy HH:mm:ss");

            mInfo.LicenseDuration = (ppvmodules[0].m_oPPVModules[0].m_oUsageModule.m_tsMaxUsageModuleLifeCycle / 60).ToString();
            mInfo.Price = mediaPrice.m_oItemPrices[0].m_oFullPrice.m_dPrice.ToString("0.00");
            mInfo.EndDate = sEndTime;                                   
            mInfo.PPVModule = mediaPrice.m_oItemPrices[0].m_sPPVModuleCode;
        }
        else
        {
            mInfo.LicenseDuration = "0";
            mInfo.Price = "0";
            mInfo.PPVModule = "0";
            mInfo.EndDate = "12/12/2030 00:00:00";
        }

        mInfo.FileID = media.FileID;
        mInfo.MediaTypeID = media.MediaTypeID;
        mInfo.TrailerURL = media.SubURL;
        mInfo.PicURL = media.PicURL;

        return mInfo;
    }
}