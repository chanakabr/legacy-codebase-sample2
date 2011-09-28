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
    private PageData pd;
    private PageContext pc;
    private XmlDocument xmlDoc;
    private MediaService m_MediaService = new MediaService();
    private SiteService m_SiteService = new SiteService();
    private string identifier;
    private PlatformType devType;

    public GWFrontController(string identifier, PlatformType devType)
    {
        this.identifier = identifier;
        this.devType = devType;

        pd = SiteMapManager.GetInstance.GetPageData(125, PlatformType.STB);
        pc = pd.GetPageByID("en", 64);

        xmlDoc = new XmlDocument();
        XmlDeclaration xmlDec = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
        xmlDoc.AppendChild(xmlDec);
    }

    protected InitializationObject GetInitObj()
    {
        InitializationObject retVal = new InitializationObject();
        retVal.Platform = devType;
        retVal.ApiUser = "tvpapi_125";
        retVal.ApiPass = "11111";
        retVal.UDID = identifier;
        return retVal;
    }

    private XmlElement createSettingsUrl(string type, string urlContent)
    {
        XmlElement subsubEle = xmlDoc.CreateElement("url");
        subsubEle.SetAttribute("type", type);
        XmlCDataSection cdata = xmlDoc.CreateCDataSection(urlContent);
        subsubEle.AppendChild(cdata);

        return subsubEle;
    }

    public object GetServiceURLs(params object[] prms)
    {
        XmlElement root = xmlDoc.CreateElement("GetServiceURLs");
        xmlDoc.AppendChild(root);

        DateTime objUTC = DateTime.Now.ToUniversalTime();
        long epoch = (objUTC.Ticks - 62135596800000000) / 10000;

        XmlElement ele = xmlDoc.CreateElement("service");
        ele.SetAttribute("date", epoch.ToString());
        XmlElement subEle = xmlDoc.CreateElement("settings");

        string baseURL = ConfigurationManager.AppSettings["BaseNetGemURL"];
        subEle.AppendChild(createSettingsUrl("base", baseURL + "/tvpapi"));
        subEle.AppendChild(createSettingsUrl("image", string.Empty));
        subEle.AppendChild(createSettingsUrl("photo", string.Empty));
        subEle.AppendChild(createSettingsUrl("content", baseURL + "/tvpapi/gateways/gateway.ashx?type=content"));
        subEle.AppendChild(createSettingsUrl("purchase", baseURL + "/tvpapi/gateways/gateway.ashx?type=purchase"));
        subEle.AppendChild(createSettingsUrl("purchaseStatus", baseURL + "/tvpapi/gateways/gateway.ashx?type=purchasestatus"));
        subEle.AppendChild(createSettingsUrl("purchaseConfirmation", baseURL + "/tvpapi/gateways/gateway.ashx?type=purchaseConfirmation"));
        subEle.AppendChild(createSettingsUrl("search-people", baseURL + "/tvpapi/gateways/searchPeoples.aspx"));
        subEle.AppendChild(createSettingsUrl("search-titles", baseURL + "/tvpapi/gateways/gateway.ashx?type=searchtitles"));
        subEle.AppendChild(createSettingsUrl("account", baseURL + "/tvpapi/gateways/gateway.ashx?type=account"));
        subEle.AppendChild(createSettingsUrl("logStreamingStart", baseURL + "/tvpapi/gateways/logStreamingStart.aspx"));
        subEle.AppendChild(createSettingsUrl("logStreamingEnd", baseURL + "/tvpapi/gateways/logdownloadend.aspx"));
        subEle.AppendChild(createSettingsUrl("addCallCenterEvent", baseURL + "/tvpapi/gateways/addCallCenterEvent.aspx"));
        subEle.AppendChild(createSettingsUrl("setlastposition", baseURL + "/tvpapi/gateways/netgem_ipvision.aspx?type=hit"));
        subEle.AppendChild(createSettingsUrl("getlastposition", baseURL + "/tvpapi/gateways/netgem_ipvision.aspx?type=getlastposition"));
        subEle.AppendChild(createSettingsUrl("logMedia", baseURL + "/tvpapi/gateways/netgem_ipvision.aspx?type=mediamark"));

        ele.AppendChild(subEle);
        root.AppendChild(ele);

        return xmlDoc.OuterXml;
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
        List<Media> lstMedias = m_MediaService.GetChannelMediaListWithMediaCount(GetInitObj(), (long)prms[0], "full", 50, 0, ref mediaCount);
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

    public object GetMediaInfo(params object[] prms)
    {
        XmlModels.GetMediaInfo mInfo = new XmlModels.GetMediaInfo();

        Media media = m_MediaService.GetMediaInfo(GetInitObj(), (long)prms[0], (int)prms[1], "480X430", true);

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
        MediaFileItemPricesContainer[] dictPrices = m_MediaService.GetItemPrices(GetInitObj(), new int[] { fileId }, false);
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

            //XXX: Fix groupID
            TVPPro.SiteManager.TvinciPlatform.Pricing.MediaFilePPVModule[] ppvmodules = new ApiPricingService(125, GetInitObj().Platform).GetPPVModuleListForMediaFiles(new int[] { fileId },
                string.Empty, string.Empty, string.Empty);
            if (ppvmodules != null && ppvmodules.Length > 0)
                sEndTime = DateTime.Now.AddMinutes(ppvmodules[0].m_oPPVModules[0].m_oUsageModule.m_tsMaxUsageModuleLifeCycle).ToString("MM/dd/yyyy HH:mm:ss");

            mInfo.LicenseDuration = (ppvmodules[0].m_oPPVModules[0].m_oUsageModule.m_tsMaxUsageModuleLifeCycle / 60).ToString();
            mInfo.Price = mediaPrice.m_oItemPrices[0].m_oFullPrice.m_dPrice.ToString("0.00");
            mInfo.EndDate = sEndTime;

            //XXX Check if needed            
            mInfo.FileID = media.FileID;
            mInfo.MediaTypeID = media.MediaTypeID;
            mInfo.PPVModule = mediaPrice.m_oItemPrices[0].m_sPPVModuleCode;
        }
        else
        {
            mInfo.LicenseDuration = "0";
            mInfo.Price = "0";
            mInfo.EndDate = "12/12/2030 00:00:00";
        }

        mInfo.TrailerURL = media.SubURL;
        mInfo.PicURL = media.PicURL;

        return mInfo;
    }
}