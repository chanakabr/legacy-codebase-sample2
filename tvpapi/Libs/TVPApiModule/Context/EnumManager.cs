using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using TVPPro.SiteManager.Context;
/// <summary>
/// Summary description for EnumManager
/// </summary>
/// 

namespace TVPApi
{
    public enum ActionType
    {
        Rate,
        Vote,
        Recommend,
        Share,
        AddFavorite,
        RemoveFavorite,
        Comment,
        Record,
        Reminder,
        Watch
    }

    public enum PriceReason
    {
        PPVPurchased,
        Free,
        ForPurchaseSubscriptionOnly,
        SubscriptionPurchased,
        ForPurchase,
        UnKnown,
        SubscriptionPurchasedWrongCurrency,
        PrePaidPurchased
    }

    [XmlType(TypeName = "TVPApiOrderBy")]
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
        Article,
        Live,
        Playlist,
        EPG,
        PrePaid
    }

    public enum LocaleUserState
    {
        [EnumAsStringValue("Unknown")]
        Unknown = 0,
        [EnumAsStringValue("Anonymous")]
        Anonymous = 1,
        [EnumAsStringValue("New")]
        New = 2,
        [EnumAsStringValue("Sub")]
        Sub = 3,
        [EnumAsStringValue("ExSub")]
        ExSub = 4,
        [EnumAsStringValue("PPV")]
        PPV = 5,
        [EnumAsStringValue("ExPPV")]
        ExPPV = 6
    }

    public enum GalleryButtonType
    {
        Unknown = 0,
        Button = 1,
        Link = 2
    }

    public enum     PlatformType
    {
        [EnumAsStringValue("Web")]
        Web,
        [EnumAsStringValue("STB")]
        STB,
        [EnumAsStringValue("iPad")]
        iPad,
        [EnumAsStringValue("ConnectedTV")]
        ConnectedTV,
        [EnumAsStringValue("Cellular")]
        Cellular,
        [EnumAsStringValue("Unknown")]
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

    public enum eOrderDirection
    {
        Asc,
        Desc
    }

    public enum eSocialPlatform
    {
        All = 0,
        InApp = 1,
        Facebook = 2,
        Twitter = 3
    }
}
