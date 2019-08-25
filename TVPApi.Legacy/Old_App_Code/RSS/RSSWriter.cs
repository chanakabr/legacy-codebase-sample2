using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.IO;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Helpers;
using System.Configuration;
using TVPApiServices;

/// <summary>
/// Summary description for RSSWriter
/// </summary>
public class RSSWriter
{
    private XmlTextWriter m_writer;

    public RSSWriter(XmlTextWriter writer)
    {
        m_writer = writer;
    }

    public void WritePageRSS(PageContext page, string nextOP)
    {
        WriteHeader();

        m_writer.WriteStartElement("channel");

        m_writer.WriteElementString("title", page.Name);

        m_writer.WriteElementString("link", string.Empty);

        m_writer.WriteElementString("description", page.Description);
        if (page.MainGalleries != null)
        {
            foreach (PageGallery pg in page.MainGalleries)
            {
                m_writer.WriteStartElement("item");

                m_writer.WriteElementString("title", pg.GroupTitle);

                m_writer.WriteElementString("description", pg.MainDescription);

                m_writer.WriteElementString("link", string.Format(nextOP, pg.GalleryID, page.ID));

                m_writer.WriteEndElement();

            }
        }

        CloseRSS();

    }

    public void WriteGalleryRSS(PageGallery pg, string sID)
    {
        WriteHeader();

        m_writer.WriteStartElement("channel");

        m_writer.WriteElementString("title", pg.GroupTitle);

        m_writer.WriteElementString("link", string.Empty);

        m_writer.WriteElementString("description", pg.MainDescription);

        foreach (GalleryItem galleryItem in pg.GalleryItems)
        {
            m_writer.WriteStartElement("item");
            int numOfItems = galleryItem.NumberOfItemsPerPage;
            m_writer.WriteElementString("title", galleryItem.Title);
            string fileType = ConfigManager.GetInstance().GetConfig(93, PlatformType.STB).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            TVMAccountType account = new PageData(93, PlatformType.STB).GetTVMAccountByUser(galleryItem.TVMUser);
            if (galleryItem.BooleanParam)
            {
                UserItemType userItemType = (UserItemType)galleryItem.NumericParam;
                string picSize = galleryItem.PictureSize;
                string[] picStr = picSize.ToLower().Split('x');
               // m_writer.WriteElementString("link", string.Concat(LinkHelper.ParseURL(System.Configuration.ConfigurationManager.AppSettings["RSSGatewayPath"]), string.Format("PTVGateway.aspx?op=GetFavoriteItems&height={0}&width={1}&itemtype=3", picStr[0], picStr[1])));
                
                MediaService mediaService = new MediaService();
                InitializationObject initObj = new InitializationObject();
                initObj.Locale = new Locale();
                initObj.SiteGuid = sID;
                initObj.Platform = PlatformType.STB;
                initObj.ApiUser = "tvpapi_93";
                initObj.ApiPass = "11111";
                List<Media> userItems = mediaService.GetUserItems(initObj, userItemType, 0, "full", 50, 0);
                HttpContext.Current.Items["Platform"] = PlatformType.STB;
                
                string mediaStr = GetIDsString(userItems);
                string rssFeed = string.Concat(TVPPro.SiteManager.Helper.SiteHelper.GetRssPathWithMediaIDS(account.BaseGroupID, mediaStr, "director,Director,actors,starring", galleryItem.PictureSize, "HIGH", fileType), "&pic_resize=1", "&with_image_in_description=0", "&index=0", string.Format("&num_of_items={0}", numOfItems));
                
                m_writer.WriteElementString("link", rssFeed);
            }
            
            else
            {

                m_writer.WriteElementString("link", string.Concat(TVPPro.SiteManager.Helper.SiteHelper.GetRssPath(account.BaseGroupID, galleryItem.TVMChannelID, galleryItem.PictureSize, "HIGH", fileType), "&pic_resize=1", "&with_image_in_description=0", "&roles=director,Director,actors,starring", "&index=0", string.Format("&num_of_items={0}", numOfItems)));
            }
            m_writer.WriteElementString("description", galleryItem.MainDescription);
            //Todo - get group ID dynamically from channel or gallery
            
           

            m_writer.WriteEndElement();

        }

        CloseRSS();
    }

    private string GetIDsString(List<Media> medias)
    {
        StringBuilder sb = new StringBuilder();
        if (medias != null)
        {
            bool isFirst = true;
            foreach (Media media in medias)
            {
                if (!isFirst)
                {
                    sb.Append(",");
                   
                }
                sb.Append(media.MediaID);
                isFirst = false;
            }
        }
        return sb.ToString();
    }

    public void WriteFavoritesGallery(List<Media> medias, string width, string height)
    {
        WriteHeader();

        m_writer.WriteStartElement("channel");

        m_writer.WriteElementString("title", "Favorites");

        m_writer.WriteElementString("link", string.Empty);

        m_writer.WriteElementString("description", "User favorites gallery");

        if (medias != null)
        {
            foreach (Media media in medias)
            {
                //Start Item
                m_writer.WriteStartElement("item");

                m_writer.WriteElementString("title", media.MediaName);

                m_writer.WriteElementString("link", media.URL);
                //Todo - get group ID dynamically from channel or gallery
                // TVMAccountType account = PageData.GetInstance(93).GetTVMAccountByUser(galleryItem.TVMUser);
                //string fileType = ConfigManager.GetInstance(93).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
                // m_writer.WriteElementString("link", string.Concat(TVPPro.SiteManager.Helper.SiteHelper.GetRssPath(account.BaseGroupID, galleryItem.TVMChannelID, galleryItem.PictureSize, "HIGH", fileType), "&pic_resize=1", "&with_image_in_description=0"));
                string description = string.Empty;
                if (!string.IsNullOrEmpty(media.Description))
                {
                    description = media.Description;

                }
                m_writer.WriteElementString("description", description);
                //Start Enclosure
                m_writer.WriteStartElement("enclosure");
                m_writer.WriteAttributeString("length", "56499232");
                m_writer.WriteAttributeString("url", media.URL);
                m_writer.WriteAttributeString("type", "video/x-ms-wmv");
                m_writer.WriteAttributeString("alt", media.MediaName);
                //End Enclosure
                m_writer.WriteEndElement();

                //Start image
                m_writer.WriteStartElement("image");
                string pic = string.Format(@"http://platform-us.tvinci.com/pic_resize_tool.aspx?h={0}&w={1}&c=true&u={2}", height, width, media.PicURL);
                m_writer.WriteElementString("url", XMLEncode(pic, true));
                m_writer.WriteEndElement();
                //End item
                m_writer.WriteEndElement();

            }
        }

        CloseRSS();
    }

    static public string XMLEncode(string sToEncode, bool bAttribute)
    {
        if (sToEncode.Length == 0)
            return string.Empty;
        //XmlAttribute element = m_xmlDox.CreateAttribute("E");
        //element.InnerText = sToEncode;
        sToEncode = sToEncode.Replace("&", "&amp;");
        sToEncode = sToEncode.Replace((char)8232, '\r');
        sToEncode = sToEncode.Replace("<", "&lt;");
        sToEncode = sToEncode.Replace(">", "&gt;");
        if (bAttribute == true)
        {
            sToEncode = sToEncode.Replace("'", "&apos;");
            sToEncode = sToEncode.Replace("\"", "&quot;");
        }
        sToEncode = sToEncode.Replace("&amp;quot;", "&quot;");
        return sToEncode;
    }

    

    public void CloseWriter()
    {
        m_writer.Flush();

        m_writer.Close();

    }


    private void WriteHeader()
    {

        m_writer.WriteStartDocument();

        m_writer.WriteStartElement("rss");

        m_writer.WriteAttributeString("version", "2.0");

    }

    private void CloseRSS()
    {
        m_writer.WriteEndElement();

        m_writer.WriteEndElement();

        m_writer.WriteEndDocument();

    }
}
