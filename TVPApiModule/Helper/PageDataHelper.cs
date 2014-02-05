using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using log4net;
using TVPApiModule.Context;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Helper;
using TVPApiModule.Objects;
using TVPApiModule.Manager;

/// <summary>
/// Summary description for PageDataHelper
/// </summary>
public class PageDataHelper
{
    private static readonly ILog logger = LogManager.GetLogger(typeof(PageDataHelper));

    private int m_groupID;
    private PlatformType m_platform;

    public PageDataHelper(int groupID, PlatformType platform)
    {

    }

    //Get specific page from site map by ID
    public static PageContext GetPageContextByID(PlatformType platform, int groupID, Locale locale, long ID, bool withMenu, bool withFooter)
    {
        PageContext retVal = null;
        List<PageContext> pages = GetPages(platform, groupID, locale);
        if (pages != null)
        {
            retVal = (from page in pages
                      where page.ID == ID
                      select page).FirstOrDefault() as PageContext;
        }

        AddMenuToPageContext(retVal, platform, locale, withMenu, withFooter, groupID);
        return retVal;
    }


    //Get specific page from site map by token
    public static PageContext GetPageContextByToken(PlatformType platform, int groupID, Locale locale, Pages token, bool withMenu, bool withFooter)
    {
        PageContext retVal = null;
        List<PageContext> pages = GetPages(platform, groupID, locale);
        if (pages != null)
        {
            retVal = (from page in pages
                      where page.PageToken.Equals(token)
                      select page).FirstOrDefault() as PageContext;
        }

        AddMenuToPageContext(retVal, platform, locale, withMenu, withFooter, groupID);
        
        return retVal;
    }

    //Get all site map pages
    public static List<PageContext> GetPages(PlatformType platform, int groupID, Locale locale)
    {
        List<PageContext> retVal = null;

        logger.InfoFormat("GetPages-> [{0}, {1}]", groupID, platform);

        TVPApiModule.Objects.SiteMap siteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, platform, locale);
        if (siteMap != null)
        {
            retVal = siteMap.GetPages();
        }

        return retVal;
    }

    //Create shallow page info object
    public static PageInfo ParsePageContextToPageInfo(PageContext page)
    {
        PageInfo retVal = new PageInfo();
        retVal.BottomProfileID = page.BottomProfileID;
        retVal.BreadCrumbText = page.BreadCrumbText;
        retVal.CarouselChannel = page.CarouselChannel;
        retVal.Description = page.Description;
        retVal.FooterID = page.FooterID;
        retVal.HasCarousel = page.HasCarousel;
        retVal.HasMiddleFooter = page.HasMiddleFooter;
        retVal.HasPlayer = page.HasPlayer;
        retVal.ID = page.ID;
        retVal.IsActive = page.IsActive;
        retVal.IsProtected = page.IsProtected;
        retVal.MenuID = page.MenuID;
        retVal.MiddleFooterID = page.MiddleFooterID;
        retVal.Name = page.Name;
        retVal.PageToken = page.PageToken;
        retVal.PlayerAutoPlay = page.PlayerAutoPlay;
        retVal.PlayerChannel = page.PlayerChannel;
        retVal.PlayerTreeCategory = page.PlayerTreeCategory;
        retVal.ProfileID = page.ProfileID;
        retVal.SideProfileID = page.SideProfileID;
        retVal.URL = page.URL;
        retVal.SetPage(page);
        return retVal;

    }

    private static void AddMenuToPageContext(PageContext page, PlatformType platform, Locale locale, bool withMenu, bool withFooter, int groupID)
    {
        if (page != null)
        {
            if (withMenu)
            {
                page.Menu = MenuHelper.GetMenuByID(platform, locale, page.MenuID, groupID);
            }
            if (withFooter)
            {
                page.Footer = MenuHelper.GetFooterByID(platform, locale, page.FooterID, groupID);
            }
        }
    }

    public static List<PageGallery> GetLocaleGalleries(List<PageGallery> PageGalleryList, Locale locale)
    {

        //Group the galleries by family num
        Dictionary<long, List<PageGallery>> galleriesByFamilies =
            (from p in PageGalleryList
             group p by p.family_id).ToDictionary(gr => gr.Key, gr => gr.ToList());

        List<PageGallery> galleryList = new List<PageGallery>();
        LocaleUserState localeUserState = locale.LocaleUserState;

        //All the galleries with default family id
        if (galleriesByFamilies.ContainsKey(0))
        {
            foreach (PageGallery pg in galleriesByFamilies[0])
            {
                if (IsGalleryInLocale(pg, locale, localeUserState))
                {
                    galleryList.Add(pg);
                }
            }
        }

        //for each family group get the highest scoring gallery (except family id 0 which we already handled)
        foreach (KeyValuePair<long, List<PageGallery>> pair in galleriesByFamilies)
        {
            if (pair.Key != 0)
            {
                PageGallery hsGallery = GetPageGalleryByLocaleScore(pair.Value, locale, localeUserState);
                if (hsGallery != null)
                {
                    galleryList.Add(hsGallery);
                }
            }
        }

        return galleryList;
    }

    //Check if a gallery fits the locale
    private static bool IsGalleryInLocale(PageGallery pg, Locale locale, LocaleUserState localeUserState)
    {
        if (pg.locale_langs != null && !string.IsNullOrEmpty(locale.LocaleLanguage) && !pg.locale_langs.Contains(locale.LocaleLanguage))
            return false;

        if (pg.locale_devices != null && !string.IsNullOrEmpty(locale.LocaleDevice) && !pg.locale_devices.Contains(locale.LocaleDevice))
            return false;

        if (pg.locale_countrys != null && !string.IsNullOrEmpty(locale.LocaleCountry) && !pg.locale_countrys.Contains(locale.LocaleCountry))
            return false;

        if (pg.locale_user_states != null && localeUserState != LocaleUserState.Unknown && !pg.locale_user_states.Contains((long)localeUserState))
            return false;

        return true;
    }

    //Get the gallery that most matches the user's locale. The locale attributes are prioreterized 
    //From high to low : Device -> Country -> Language -> User State
    private static PageGallery GetPageGalleryByLocaleScore(List<PageGallery> galleries, Locale locale, LocaleUserState localeUserState)
    {
        PageGallery retVal = null;
        double prevScore = 0;
        string localeLanguage = locale.LocaleLanguage;
        string localeCountry = locale.LocaleCountry;
        string localeDevice = locale.LocaleDevice;

        //Country userCountry = TVPPro.SiteManager.Services.UsersService.Instance.GetIPToCountry(ip);    
        foreach (PageGallery gallery in galleries)
        {
            double currentScore = 0;
            if (gallery.locale_user_states != null)
            {
                if (gallery.locale_user_states.Contains((int)localeUserState))
                    currentScore += Math.Pow(2, 0);
                else
                    continue;
            }

            if (gallery.locale_langs != null)
            {
                if (gallery.locale_langs.Contains(localeLanguage))
                    currentScore += Math.Pow(2, 1);
                else
                    continue;
            }

            if (gallery.locale_countrys != null)
            {
                if (gallery.locale_countrys.Contains(localeCountry))
                    currentScore += Math.Pow(2, 2);
                else
                    continue;
            }

            if (gallery.locale_devices != null)
            {
                if (gallery.locale_devices.Contains(localeDevice))
                    currentScore += Math.Pow(2, 3);
                else
                    continue;
            }

            //This means that this is a default gallery and we havnt found a precise match yet
            if (currentScore == 0 && retVal == null)
            {
                retVal = gallery;
            }
            else if (currentScore > prevScore)
            {
                retVal = gallery;
                prevScore = currentScore;
            }
        }

        return retVal;
    }





}
