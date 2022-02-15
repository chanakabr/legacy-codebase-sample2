using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Phx.Lib.Log;
using TVPApi;

/// <summary>
/// Summary description for PageDataHelper
/// </summary>
public class PageDataHelper
{
    private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    //private int m_groupID;
    //private PlatformType m_platform;

    public PageDataHelper(int groupID, PlatformType platform)
    {

    }

    //Get specific page from site map by ID
    public static TVPApi.PageContext GetPageContextByID(InitializationObject initObj, int groupID, long ID, bool withMenu, bool withFooter)
    {
        PageContext retVal = null;
        List<PageContext> pages = GetPages(initObj, groupID);
        if (pages != null)
        {
            retVal = (from page in pages
                      where page.ID == ID
                      select page).FirstOrDefault() as PageContext;
        }

        AddMenuToPageContext(retVal, initObj, withMenu, withFooter, groupID);
        return retVal;
    }


    //Get specific page from site map by token
    public static TVPApi.PageContext GetPageContextByToken(InitializationObject initObj, int groupID, TVPApi.Pages token, bool withMenu, bool withFooter)
    {
        PageContext retVal = null;
        List<PageContext> pages = GetPages(initObj, groupID);
        if (pages != null)
        {
            retVal = (from page in pages
                      where page.PageToken.Equals(token)
                      select page).FirstOrDefault() as PageContext;
        }

        AddMenuToPageContext(retVal, initObj, withMenu, withFooter, groupID);

        return retVal;
    }

    //Get all site map pages
    public static List<PageContext> GetPages(InitializationObject initObj, int groupID)
    {
        List<PageContext> retVal = null;

        logger.InfoFormat("GetPages-> [{0}, {1}]", groupID, initObj.Platform);

        TVPApi.SiteMap siteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
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

    private static void AddMenuToPageContext(PageContext page, InitializationObject initObj, bool withMenu, bool withFooter, int groupID)
    {
        if (page != null)
        {
            if (withMenu)
            {
                page.Menu = MenuHelper.GetMenuByID(initObj, page.MenuID, groupID);
            }
            if (withFooter)
            {
                page.Footer = MenuHelper.GetFooterByID(initObj, page.FooterID, groupID);
            }
        }
    }

    public static List<PageGallery> GetLocaleGalleries(List<PageGallery> PageGalleryList, Locale locale)
    {

        //Group the galleries by family num
        Dictionary<long, List<PageGallery>> galleriesByFamilies =
            (from p in PageGalleryList
             group p by p.FamilyID).ToDictionary(gr => gr.Key, gr => gr.ToList());

        List<PageGallery> galleryList = new List<PageGallery>();
        TVPApi.LocaleUserState localeUserState = locale.LocaleUserState;

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
    private static bool IsGalleryInLocale(PageGallery pg, Locale locale, TVPApi.LocaleUserState localeUserState)
    {
        if (pg.Locale_Langs != null && !string.IsNullOrEmpty(locale.LocaleLanguage) && !pg.Locale_Langs.Contains(locale.LocaleLanguage))
            return false;

        if (pg.Locale_Devices != null && !string.IsNullOrEmpty(locale.LocaleDevice) && !pg.Locale_Devices.Contains(locale.LocaleDevice))
            return false;

        if (pg.Locale_Countrys != null && !string.IsNullOrEmpty(locale.LocaleCountry) && !pg.Locale_Countrys.Contains(locale.LocaleCountry))
            return false;

        if (pg.Locale_UserStates != null && localeUserState != TVPApi.LocaleUserState.Unknown && !pg.Locale_UserStates.Contains((long)localeUserState))
            return false;

        return true;
    }

    //Get the gallery that most matches the user's locale. The locale attributes are prioreterized 
    //From high to low : Device -> Country -> Language -> User State
    private static PageGallery GetPageGalleryByLocaleScore(List<PageGallery> galleries, Locale locale, TVPApi.LocaleUserState localeUserState)
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
            if (gallery.Locale_UserStates != null)
            {
                if (gallery.Locale_UserStates.Contains((int)localeUserState))
                    currentScore += Math.Pow(2, 0);
                else
                    continue;
            }

            if (gallery.Locale_Langs != null)
            {
                if (gallery.Locale_Langs.Contains(localeLanguage))
                    currentScore += Math.Pow(2, 1);
                else
                    continue;
            }

            if (gallery.Locale_Countrys != null)
            {
                if (gallery.Locale_Countrys.Contains(localeCountry))
                    currentScore += Math.Pow(2, 2);
                else
                    continue;
            }

            if (gallery.Locale_Devices != null)
            {
                if (gallery.Locale_Devices.Contains(localeDevice))
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
