using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TVPApi;
using System.IO;
using System.Xml;
using System.Text;
using System.Web.Script.Serialization;
using TVPApiServices;

public partial class TestPage : System.Web.UI.Page
{
    SiteService m_siteService = new SiteService();
    MediaService m_mediaService = new MediaService();

    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void SiteMapTestClk(object source, EventArgs e)
    {   
        // System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        ///StringBuilder sb = new StringBuilder();
        InitializationObject initObj = new InitializationObject();
        initObj.Locale = new Locale();
        //initObj.Locale.LocaleCountry = CountryTxt.Text;
        //initObj.Locale.LocaleDevice = DeviceTxt.Text;
        initObj.Locale.LocaleLanguage = LanguageText.Text;
        initObj.Locale.LocaleUserState = LocaleUserState.Unknown;
        initObj.SiteGuid = SiteGuidTxt.Text;
        if (!string.IsNullOrEmpty(PlatformTxt.Text))
        {
            initObj.Platform = (PlatformType)Enum.Parse(typeof(PlatformType), PlatformTxt.Text);
        }
        // serializer.Serialize(initObj, sb);
        //initObj.Platform = PlatformTxt.Text;
        string user = UserNameTxt.Text;
        string pass = PassTxt.Text;
        JavaScriptSerializer jsSer = new JavaScriptSerializer();
        StringBuilder sb = new StringBuilder();
        jsSer.Serialize(initObj, sb);
        //TVPPro.SiteManager.DataLoaders.CategoryTreeLoader catTree = new TVPPro.SiteManager.DataLoaders.CategoryTreeLoader(672);
        //catTree.Execute();
       // Category cat = service.GetCategory(initObj, user, pass, 689);
       // List<Media> channelMedia = service.GetChannelMediaList(initObj, user, pass, cat.Channels[0].ChannelID, "full", 5, 0);
        //service.GetMediaInfo(initObj, user, pass, 98030, 181, "full", true);
        //service.SearchMediaByTag(initObj, "tvpapi_93", "11111", "starring", "Plutarco Haza", 181, "full", 5, 0, OrderBy.Rating);
        //service.GetPeopleWhoWatched(initObj, "tvpapi_93", "11111", 88125, 181, "full", 5, 0);
        //TVPApi.SiteMap map = service.GetSiteMap(initObj, user, pass);
        //service.ActionDone(initObj, user, pass, ActionType.RemoveFavorite, 122632, 213, 0);
       // service.GetMediaInfo(initObj, "tvpapi_123", "11111", 122950, 257, "full", true);
        //service.GetCategory(initObj, user, pass, 961);
        //TVPApi.SiteMap siteMap = service.GetSiteMap(initObj, user, pass);
       // service.SearchMedia(initObj, user, pass, "t", 0, "full", 10, 0, OrderBy.Added);
        //service.GetUserItems(initObj, user, pass, UserItemType.Favorite, 0, "full", 10, 0);
        //service.GetGalleryItemContent(initObj, user, pass, 1910, 191, 67, "full", 5, 0);
        //long count = 0;
        //service.GetRelatedMediaWithMediaCount(initObj, user, pass, 122682, 0, "full", 5, 0, ref count);
        //service.SignIn(initObj, user, pass, "test3@test.tt", "123456");
        m_mediaService.GetMediaInfo(initObj, 122632, 257, "full", true);
        //Media media = service.GetMediaInfo(initObj, user, pass, 122620, 257, "full", true);
       // bool added = service.ActionDone(initObj, user, pass, ActionType.AddFavorite, 83905, 181);
        //bool isFavorite = service.IsMediaFavorite(initObj, user, pass, 88536);
       // List<Media> purchased = service.GetUserItems(initObj, user, pass, UserItemType.Favorite, 0, "full", 20, 0);
        //List<Media> favorites = service.GetUserItems(initObj, user, pass, UserItemType.Favorite, 0, "full", 20, 0);
        //Media media = service.GetMediaInfo(initObj, user, pass, 101223, 181, "full", true);
       // string guid = service.SignIn(initObj, user, pass, "idow@gmail.com", "eliron27");
       // List<Media> serachValues = service.SearchMedia(initObj, user, pass, "Sos", 0, "200x112", 20, 0);
       //  List<Media> medias = service.GetGalleryItemContent(initObj, user, pass, 1910, 191, 67, "full", 10, 1);
        //List<Comment> comments = service.GetMediaComments(initObj, user, pass, 101022, 5, 0);
        //List<Media> related = service.GetRelatedMedias(initObj, user, pass, 83905, 181, "full", 5, 0);
        //List<Media> peopleWhoWatched = service.GetPeopleWhoWatched(initObj, user, pass, 88486, "200x112", 5, 0);
        //List<Media> favorites = service.GetUserItems(initObj, user, pass, UserItemType.Favorite, "123456", 0, "200x112", 5, 0);
        //service.ActionDone(initObj, user, pass, ActionType.RemoveFavorite, 83905, 181, "123456");
        //List<Media> newFavorites = service.GetUserItems(initObj, user, pass, UserItemType.Favorite, "123456", 0, "200x112", 5, 0);
        //List<Media> search = service.SearchMediaByTag(initObj, user, pass, "Series name", "Sos mi vida", 181, "full", 20, 0, OrderBy.ABC);
       // //UsersXMLParser parser = UsersXMLParser.Instance;
        //List<Media> favorites = service.GetUserItems(initObj, user, pass, UserItemType.Favorite, "99414", 1, "200x112", 20, 0);
        //serializer.Serialize(siteMap, sb);
        //List<Media> medias = service.GetRelatedMedias(initObj, user, pass, 83869, 182, "200x112", 20, 0);
        //System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(siteMap.GetType());
        //  XmlTextWriter textWriter = new XmlTextWriter("C:\\sitemap.xml", System.Text.Encoding.UTF8);
        //  serializer.Serialize(textWriter, siteMap);

    }

    //protected void GetPageTestClk(object source, EventArgs e)
    //{
    //    if (service == null)
    //    {
    //        service = new Service();
    //    }
    //    InitializationObject initObj = new InitializationObject();
    //    initObj.Locale = new Locale();
    //    initObj.Locale.LocaleCountry = CountryTxt.Text;
    //    initObj.Locale.LocaleDevice = DeviceTxt.Text;
    //    initObj.Locale.LocaleLanguage = LanguageText.Text;
    //    initObj.Locale.SiteGuid = SiteGuidTxt.Text;
    //    initObj.Platform = PlatformTxt.Text;
    //    bool withMenu;
    //    bool.TryParse(WithMenuTxt.Text, out withMenu);
    //    bool withFooter;
    //    bool.TryParse(WithFooterTxt.Text, out withFooter);
    //    long PageID = Convert.ToInt32(PageIDTxt.Text);
    //    string user = UserNameTxt.Text;
    //    string pass = PassTxt.Text;
    //    PageContext page = service.GetPage(initObj, user, pass, PageID, withMenu, withFooter);
    //    TVPApi.Menu menu = page.Menu;
    //}

    //protected void GetGalleryContentClk(object source, EventArgs e)
    //{
    //    if (service == null)
    //    {
    //        service = new Service();
    //    }
    //    InitializationObject initObj = new InitializationObject();
    //    initObj.Locale = new Locale();
    //    initObj.Locale.LocaleCountry = CountryTxt.Text;
    //    initObj.Locale.LocaleDevice = DeviceTxt.Text;
    //    initObj.Locale.LocaleLanguage = LanguageText.Text;
    //    initObj.Locale.SiteGuid = SiteGuidTxt.Text;
    //    initObj.Platform = PlatformTxt.Text;
    //    long PageID = Convert.ToInt32(GetContentPageIDTxt.Text);
    //    long GalleryID = Convert.ToInt32(GalleryIDText.Text);
    //    string picSize = PicSizeTxt.Text;
    //    string user = UserNameTxt.Text;
    //    string pass = PassTxt.Text;
    //    List<GalleryItem> mediaList = service.GetGalleryContent(initObj, user, pass, GalleryID, PageID, picSize, 0, 0);
    //}

    //protected void GetGalleryItemContentClk(object source, EventArgs e)
    //{
    //    if (service == null)
    //    {
    //        service = new Service();
    //    }
    //    InitializationObject initObj = new InitializationObject();
    //    initObj.Locale = new Locale();
    //    initObj.Locale.LocaleCountry = CountryTxt.Text;
    //    initObj.Locale.LocaleDevice = DeviceTxt.Text;
    //    initObj.Locale.LocaleLanguage = LanguageText.Text;
    //    initObj.Locale.SiteGuid = SiteGuidTxt.Text;
    //    initObj.Platform = PlatformTxt.Text;
    //    long PageID = Convert.ToInt32(GalleyItemContentPageIdTxt.Text);
    //    long GalleryID = Convert.ToInt32(GalleyItemContentGalleryIdTxt.Text);
    //    long GalleryItemID = Convert.ToInt32(GalleryItemIDTxt.Text);
    //    string picSize = GalleryItemContentPicSizeTxt.Text;
    //    string user = UserNameTxt.Text;
    //    string pass = PassTxt.Text;
    //    int pageSize = Convert.ToInt32(PageSizeTxt.Text);
    //    int startIndex = Convert.ToInt32(StartIndexTxt.Text);
    //    List<Media> mediaList = service.GetGalleryItemContent(initObj, user, pass, GalleryItemID, GalleryID, PageID, picSize, pageSize, startIndex);
    //}

    //protected void GetMediaInfoTestClk(object source, EventArgs e)
    //{
    //    if (service == null)
    //    {
    //        service = new Service();
    //    }
    //    InitializationObject initObj = new InitializationObject();
    //    initObj.Locale = new Locale();
    //    initObj.Locale.LocaleCountry = CountryTxt.Text;
    //    initObj.Locale.LocaleDevice = DeviceTxt.Text;
    //    initObj.Locale.LocaleLanguage = LanguageText.Text;
    //    initObj.Locale.SiteGuid = SiteGuidTxt.Text;
    //    initObj.Platform = PlatformTxt.Text;
    //    long MediaID = Convert.ToInt32(MediaIDTxt.Text);
    //    int MediaType = Convert.ToInt32(MediaTypeTxt.Text);
    //    string picSize = MediaInfoPicSizeTxt.Text;
    //    string user = UserNameTxt.Text;
    //    string pass = PassTxt.Text;
    //    Media media = service.GetMediaInfo(initObj, string.Empty, string.Empty, MediaID, MediaType, picSize);
    //}

    //protected void SearchMediaClk(object source, EventArgs e)
    //{
    //    if (service == null)
    //    {
    //        service = new Service();
    //    }
    //    InitializationObject initObj = new InitializationObject();
    //    initObj.Locale = new Locale();
    //    initObj.Locale.LocaleCountry = CountryTxt.Text;
    //    initObj.Locale.LocaleDevice = DeviceTxt.Text;
    //    initObj.Locale.LocaleLanguage = LanguageText.Text;
    //    initObj.Locale.SiteGuid = SiteGuidTxt.Text;
    //    initObj.Platform = PlatformTxt.Text;
    //    int MediaType = Convert.ToInt32(SearchMediaTypeTxt.Text);
    //    string picSize = SearchPicSize.Text;
    //    string tagName = TagNameTxt.Text;
    //    string tagValue = TagValueTxt.Text;
    //    string user = UserNameTxt.Text;
    //    string pass = PassTxt.Text;
    //    List<Media> retVal = service.SearchMedia(initObj, user, pass, tagName, tagValue, MediaType, picSize, 20, 0);
    //}

    protected void ClearCacheClk(object source, EventArgs e)
    {
        Response.Redirect("~\\ClearCache.aspx");
    }

}
