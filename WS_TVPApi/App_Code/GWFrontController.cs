using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using System.Xml;
using TVPApiServices;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPApiModule.Services;

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

    public string GetAllChannels(params object[] prms)
    {
        XmlElement root = xmlDoc.CreateElement("GetAllChannels");
        xmlDoc.AppendChild(root);

        foreach (PageGallery pg in pc.MainGalleries)
        {
            foreach (GalleryItem gi in pg.GalleryItems)
            {
                XmlElement channelEle = xmlDoc.CreateElement("Channel");
                XmlElement ele = xmlDoc.CreateElement("Title");
                ele.InnerText = gi.Title;
                channelEle.AppendChild(ele);

                ele = xmlDoc.CreateElement("Tvmch");
                ele.InnerText = gi.TVMChannelID.ToString();
                channelEle.AppendChild(ele);

                ele = xmlDoc.CreateElement("Picsize");
                ele.InnerText = gi.PictureSize;
                channelEle.AppendChild(ele);

                root.AppendChild(channelEle);
            }
        }

        return xmlDoc.OuterXml;
    }

    public string GetChannelMedias(params object[] prms)
    {
        XmlElement root = xmlDoc.CreateElement("GetChannelMedias");
        xmlDoc.AppendChild(root);

        long mediaCount = 0;
        List<Media> lstMedias = m_MediaService.GetChannelMediaListWithMediaCount(GetInitObj(), (long)prms[0], "full", 50, 0, ref mediaCount);

        if (lstMedias != null)
        {
            foreach (Media item in lstMedias)
            {
                XmlElement ele = xmlDoc.CreateElement("Media");
                ele.SetAttribute("ID", item.MediaID);
                ele.SetAttribute("Type", item.MediaTypeID);
                root.AppendChild(ele);
            }
        }

        return xmlDoc.OuterXml;
    }

    public string GetMediaInfo(params object[] prms)
    {
        XmlElement root = xmlDoc.CreateElement("GetMediaInfo");
        xmlDoc.AppendChild(root);

        Media media = m_MediaService.GetMediaInfo(GetInitObj(), (long)prms[0], (int)prms[1], "480X430", true);

        //XXX Error handling
        if (media == null)
            return root.OuterXml;        

        XmlElement ele = xmlDoc.CreateElement("Description");        
        //XXX Removing that annoying replace!
        ele.InnerText = media.Description.Replace(@"<\p>", " ");
        root.AppendChild(ele);

        ele = xmlDoc.CreateElement("MediaID");
        ele.InnerText = media.MediaID;
        root.AppendChild(ele);

        ele = xmlDoc.CreateElement("Title");
        ele.InnerText = media.MediaName.Replace('(', ' ').Replace(')', ' ');
        root.AppendChild(ele);

        string runtime = (from meta in media.Metas where meta.Key.Equals("Display run time") select meta.Value).FirstOrDefault();
        ele = xmlDoc.CreateElement("Duration");
        if (!string.IsNullOrEmpty(runtime))
        {
            string[] time = runtime.Split(new char[] { 'h', 'm' });
            ele.InnerText = string.Format("{0:0}", int.Parse(time[0]) * 60 + int.Parse(time[1]));
        }
        else
            ele.InnerText = "0";

        root.AppendChild(ele);

        ele = xmlDoc.CreateElement("Rating");
        ele.InnerText = media.Rating.ToString();
        root.AppendChild(ele);

        ele = xmlDoc.CreateElement("Copyright");
        ele.InnerText = (from meta in media.Metas where meta.Key.Equals("Production Name") select meta.Value).FirstOrDefault() ?? "None";
        root.AppendChild(ele);

        ele = xmlDoc.CreateElement("Adult");
        ele.InnerText = "false";
        root.AppendChild(ele);

        ele = xmlDoc.CreateElement("ProductionYear");
        ele.InnerText = (from meta in media.Metas where meta.Key.Equals("Release year") select meta.Value).FirstOrDefault() ?? "None";
        root.AppendChild(ele);

        ele = xmlDoc.CreateElement("HD");
        ele.InnerText = "true";
        root.AppendChild(ele);

        ele = xmlDoc.CreateElement("Country");
        string countries = (from tag in media.Tags where tag.Key.Equals("Country") select tag.Value).FirstOrDefault();
        if (countries != null)
        {
            foreach (string country in countries.Split('-'))
            {
                XmlElement subEle = xmlDoc.CreateElement("Name");
                subEle.InnerText = country;
                ele.AppendChild(subEle);
            }
        }
        root.AppendChild(ele);

        ele = xmlDoc.CreateElement("Actors");
        string actors = (from tag in media.Tags where tag.Key.Equals("Cast") select tag.Value).FirstOrDefault();
        if (actors != null)
        {
            foreach (string actor in actors.Split('|'))
            {
                XmlElement subEle = xmlDoc.CreateElement("Name");
                subEle.InnerText = actor;
                ele.AppendChild(subEle);
            }
        }
        root.AppendChild(ele);

        ele = xmlDoc.CreateElement("Directors");
        string directors = (from tag in media.Tags where tag.Key.Equals("Director") select tag.Value).FirstOrDefault();
        if (directors != null)
        {
            foreach (string director in directors.Split('-'))
            {
                XmlElement subEle = xmlDoc.CreateElement("Name");
                subEle.InnerText = director;
                ele.AppendChild(subEle);
            }
        }
        root.AppendChild(ele);

        ele = xmlDoc.CreateElement("URL");
        ele.InnerText = media.URL;
        root.AppendChild(ele);

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

            ele = xmlDoc.CreateElement("LicenseDuration");
            ele.InnerText = (ppvmodules[0].m_oPPVModules[0].m_oUsageModule.m_tsMaxUsageModuleLifeCycle / 60).ToString();
            root.AppendChild(ele);

            ele = xmlDoc.CreateElement("Price");
            ele.InnerText = mediaPrice.m_oItemPrices[0].m_oFullPrice.m_dPrice.ToString("#.##");
            root.AppendChild(ele);

            ele = xmlDoc.CreateElement("EndDate");
            ele.InnerText = sEndTime;
            root.AppendChild(ele);

            //XXX Check if needed
            ele = xmlDoc.CreateElement("FileID");
            ele.InnerText = media.FileID;
            root.AppendChild(ele);
            ele = xmlDoc.CreateElement("MediaTypeID");
            ele.InnerText = media.MediaTypeID;
            root.AppendChild(ele);
            ele = xmlDoc.CreateElement("PPVModule");
            ele.InnerText = mediaPrice.m_oItemPrices[0].m_sPPVModuleCode;
            root.AppendChild(ele);
        }
        else
        {
            ele = xmlDoc.CreateElement("LicenseDuration");
            ele.InnerText = "0";
            root.AppendChild(ele);

            ele = xmlDoc.CreateElement("Price");
            ele.InnerText = "0";
            root.AppendChild(ele);

            ele = xmlDoc.CreateElement("EndDate");
            ele.InnerText = "12/12/2090 00:00:00";
            root.AppendChild(ele);
        }

        ele = xmlDoc.CreateElement("TrailerURL");
        ele.InnerText = media.SubURL;
        root.AppendChild(ele);

        ele = xmlDoc.CreateElement("PicURL");
        ele.InnerText = media.PicURL;
        root.AppendChild(ele);

        return root.OuterXml;
    }
}