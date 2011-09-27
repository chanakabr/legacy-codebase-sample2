using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using System.Xml;
using TVPApiServices;

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
        List<Media> lstMedias = m_MediaService.GetChannelMediaListWithMediaCount(GetInitObj(), (long) prms[0], "full", 50, 0, ref mediaCount);

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
}