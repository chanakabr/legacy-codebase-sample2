using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.Context
{
    public static class Enums
    {
        public enum eGalleryLocation
        {
            Main,
            Side,
            Top,
            Bottom
        }

        public enum eUserOnlineStatus
        {
            LoggedOut,
            LoggedIn,
            Recognised,
            Error,
            NotActive,
            NotValidInfo,
            Locked,
            UserDoesNotExist,
            UserAllreadyLoggedIn
        }

        public enum eLocaleUserState
        {
            Unknown = 0,
            Anonymous = 1,
            New = 2,
            Sub = 3,
            ExSub = 4,
            PPV = 5,
            ExPPV = 6
        }

        public enum eGalleryButtonType
        {
            Unknown = 0,
            Button = 1,
            Link = 2,
        }
        public enum eGalleryType
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

        public enum eUIGalleryType
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
            AdsGallery1 = 21,
            FreeTextImage = 23,
            TagsGallery = 24,
            PictureGallery = 25,
            DetailsXPack = 26,
            VlinkGallery = 27,
            DetailsGallery = 28,
            MiniSpotlight = 29,
            AdsGallery2 = 30,
            DefaultChannelGallery = 31,
            ChannelGallery = 32,
            Comments = 33,
            PromotedPackages = 34,
            TwitterSide = 35,
            FacebookSide = 36,
            FollowUsSide = 37,
            OnSocialSide = 38,
            DynamicLinkImage = 39,
            FacebookLanding = 40,
            EpgGallery = 41,
            EpgCarouselGallery = 42,
            SurveyGallery = 43,
            FriendsGallery = 44,
            RecommendedGallery = 45,
            RSSGallery = 46,
            FacebookCanvasGallery = 47,
            DynamicIframGallery = 49

        }

        public enum eViewType
        {
            Carousel,
            List,
            Grid,
            Details
        }

        public enum eSiteMode
        {
            Regular,
            Editorial
        }

        public enum ePages
        {
            UnKnown = 0,
            HomePage = 1,
            MediaPage = 2,
            MediaList = 3,
            Search = 4,
            MyZone = 5,
            Dynamic = 6,
            ShowPage = 7,
            Static = 8,
            Page404 = 9,
            Package = 10,
            MyPlayList = 11,
            LinearChannel = 12,
            ShowsLobby = 13,
            AllTags = 14,
            Meta = 15,
            Purchase = 16,
            LoginJoin = 17,
            PackagesLobby = 18,
            Messages = 19,
            Article = 20,
            SelfCare = 21,
            PrePaid = 22,
            Live = 23,
            Playlist = 24,
            RegistrationPage = 25,
            FacebookCanvas = 26,
            EPGPage = 27,
            VodPage = 28,
            Category = 29,
            SeasonsPage = 30,
            ChannelPage = 31,
            DeviceDetectionPage = 32,
            Settings = 33,
            PersonalZone = 34,
            Actor = 35
        }

        public enum eOrderBy
        {
            None = 0,
            Added = 1,
            Views = 2,
            Rating = 3,
            ABC = 4,
            Meta = 5
        }

        public enum eAddToSide
        {
            Right,
            Left
        }

        public enum eOrderDirection
        {
            Asc,
            Desc
        }

        public enum eFavoriteItemTypes
        {
            Season = 1,
            Media = 2
        }

        public enum MediaListType
        {
            MediaList = 1,
            TagPairList = 2,
            TagList = 3
        }

        public enum eSortByOptions
        {
            [EnumAsStringValue("Newest")]
            Newest,
            [EnumAsStringValue("Most Viewed")]
            MostViewed,
            [EnumAsStringValue("Most Rated")]
            MostRated,
            [EnumAsStringValue("Highest Rated")]
            HighestRated,
            [EnumAsStringValue("A-Z")]
            AlfaBet,
            [EnumAsStringValue("Most Reviewed")]
            MostReviewed,
        }

        public enum eCustomLayoutItemType
        {
            Show = 1,
            Page = 2
        }

        public enum eCustomLayoutPictureRepeat
        {
            X,
            Y,
            XY
        }

        public enum eSearchType
        {
            ByKeyword,
            ByTag,
            ByType
        }

        public enum eMediaSource
        {
            tvm,
            tvs
        }

        public enum eAccountType
        {
            All = 5,
            Regular = 1,
            Fictivic = 3,
            UGC = 4,
            Adult = 2,
            Parent = 6
        }

        public enum eBrandingRecurringType
        {
            None = 0,
            Horizontal = 1,
            Vertical = 2,
            Both = 3
        }

        public enum ePriceReason
        {
            PPVPurchased,
            Free,
            ForPurchaseSubscriptionOnly,
            SubscriptionPurchased,
            ForPurchase,
            UnKnown,
            SubscriptionPurchasedWrongCurrency
        }

        public enum eLinkType
        {
            Tag,
            Meta
        }

        public enum eSocialPlatform
        {
            UNKNOWN = 0,
            FACEBOOK = 1,
            GOOGLE = 2
        }

        public enum eSocialAction
        {
            UNKNOWN = 0,
            LIKE = 1,
            UNLIKE = 2,
            SHARE = 3,
            POST = 4
        }

        public enum ePlatform
        {
            Web,
            STB,
            iPad,
            ConnectedTV,
            Cellular,
            Unknown
        }

        public enum eAgeRule
        {
            None,
            NC16,
            M18,
            R21
        }
    }
}
