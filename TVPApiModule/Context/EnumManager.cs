using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for EnumManager
/// </summary>
/// 

namespace TVPApi
{
    public enum ActionType
    {
        Rate,
        Recommend,
        Share,
        AddFavorite,
        RemoveFavorite,
        Comment,
        Record,
        Reminder,
        Watch,
        Like
    }

    public enum PriceReason
    {
        PPVPurchased,
        Free,
        ForPurchaseSubscriptionOnly,
        SubscriptionPurchased,
        ForPurchase,
        UnKnown,
        SubscriptionPurchasedWrongCurrency
    }

    public enum OrderBy
    {
        None = 0,
        Added = 1,
        Views = 2,
        Rating = 3,
        ABC = 4,
        Meta = 5
    }

    public enum AccountType
    {
        Parent = 5,
        Regular = 1,
        Fictivic = 3,
        UGC = 4,
        Adult = 2
    }

    public enum GalleryLocation
    {
        Main,
        Side,
        Top,
        Bottom
    }

    public enum GalleryType
    {
        Movie,
        Show,
        Music,
        Episode,
        ShortFilms,
        Documentries,
        BehindTheFilms,
        EducationMaterial,
        Clip,
        LiveEvent
    }

    public enum UIGalleryType
    {
        Carousel = 1,
        XPack = 2,
        Free = 3,
        Top = 4,
        Player = 5,
        Spotlight = 6,
        MovingGallery = 7,
        RandomGallery = 8,
        Cards = 9,
        MovieFinder = 10,
        RelPackages = 11,
        MyZoneMiniFeed = 12,
        PeopleWhoWatched = 13,
        BannerBig = 14,
        Banner = 15,
        LinearChannel = 16,
        Multi = 17,
        MostViewd = 18,
        EditorialComments = 19,
        FreeFlash = 20,
        AdsGallery = 21
    }

    public enum Pages
    {
        UnKnown,
        HomePage,
        MediaPage,
        MediaList,
        Search,
        MyZone,
        Dynamic,
        ShowPage,
        Static,
        Page404,
        Package,
        MyPlayList,
        LinearChannel,
        ShowsLobby,
        AllTags,
        Meta,
        Purchase,
        LoginJoin,
        PackagesLobby,
        Messages,
        Article
    }

    public enum LocaleUserState
    {
        Unknown = 0,
        Anonymous = 1,
        New = 2,
        Sub = 3,
        ExSub = 4,
        PPV = 5,
        ExPPV = 6
    }

    public enum GalleryButtonType
    {
        Unknown = 0,
        Button = 1,
        Link = 2
    }

    public enum PlatformType
    {
        Web,
        STB,
        iPad,
        ConnectedTV,
        Cellular,
        Unknown
    }

    public enum FormatType
    {
        SOAP,
        XML,
        JSON
    }

    public enum UserItemType
    {
        Rental = 0,
        Subscription = 1,
        Package = 2,
        Favorite = 3,
        All = 4
    }

}
