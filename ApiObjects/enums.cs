using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ApiObjects
{
    public enum eHttpRequestType
    {
        Post,
        Get
    }

    public enum UserStatus
    {
        NotRelevant = 0,
        Anonymus = 1,
        New = 2,
        Sub = 3,
        ExSub = 4,
        PPVHolder = 5,
        EXPPVHolder = 6
    }

    public enum AdminUserStatus
    {
        Unknown = 0,
        UserDoesNotExist = 1,
        UserALreadyLoggedIn = 2,
        UserLocked = 3,
        OK = 4,
        Error = 5
    }

    public enum eCutType
    {
        And = 0,
        Or = 1
    }

    public enum eBlockType
    {
        Allowed = 0,
        Validation = 1,
        AgeBlock = 2,
        Geo = 3,
        Device = 4,
        UserType = 5,
        AnonymousAccessBlock = 6
    }

    public enum RuleType
    {
        Parental = 0,
        Geo = 1,
        UserType = 2,
        Device = 3
    }

    public enum eMailTemplateType
    {
        Welcome = 0,
        ForgotPassword = 1,
        Purchase = 2,
        SendToFriend = 3,
        Token = 4,
        PaymentFail = 5,
        SendPassword = 6,
        ChangedPin = 7,
        Notification = 8,
        AddUserToDomain = 9,
        ChangePassword = 10,
        PreviewModuleCancelOrRefund = 11,
        PurchaseWithPreviewModule = 12,
        AddDeviceToDomain = 13
    }

    public enum eGroupRuleType
    {
        Unknown = 0,
        Parental = 1,
        Purchase = 2,
        Device = 3,
        EPG = 4
    }

    [Serializable]
    public enum eObjectType
    {
        [EnumMember]
        Unknown = -1,
        [EnumMember]
        Media = 0,
        [EnumMember]
        Channel = 1,
        [EnumMember]
        EPG = 2,
        [EnumMember]
        EpgChannel = 3,
        [EnumMember]
        Recording = 4
    }

    [Serializable]
    public enum eCaSystem
    {
        [EnumMember]
        OTT = 0,

        [EnumMember]
        OVP = 1
    }

    [Serializable]
    public enum eAction
    {
        [EnumMember]
        Off = 0,
        [EnumMember]
        On = 1,
        [EnumMember]
        Update = 2,
        [EnumMember]
        Delete = 3,
        [EnumMember]
        Rebuild = 4
    }

    [Serializable]
    public enum EpgSearchType
    {
        [EnumMember]
        ByDate,
        [EnumMember]
        Current
    }

    public enum StatsType
    {
        MEDIA,
        EPG
    }

    [Serializable]
    public enum Language
    {
        [EnumMember]
        English,
        [EnumMember]
        Hebrew,
        [EnumMember]
        Russian,
        [EnumMember]
        Arabic
    }

    [Serializable]
    public enum eOperatorEvent : byte
    {
        [EnumMember]
        ChannelAddedToSubscription = 0,
        [EnumMember]
        ChannelRemovedFromSubscription = 1,
        [EnumMember]
        SubscriptionAddedToOperator = 2,
        [EnumMember]
        SubscriptionRemovedFromOperator = 3
    }

    public enum DomianEnvironmentType
    {
        SUS,
        MUS
    }


    public enum eCacheGroupType
    {
        GroupCacheExternal,
        GroupCacheInternal
    }
    public enum Btype
    {
        SUBSCRIPTION = 0,
        COLLECTION = 1
    }


    [Serializable]
    public enum eTransactionType
    {
        [EnumMember]
        PPV,
        [EnumMember]
        Subscription,
        [EnumMember]
        Collection
    }

    public enum eBusinessModule
    {
        PPV = 1,
        Subscription = 2
    }

    public enum FieldTypes
    {
        Unknown,
        Basic,
        Meta,
        Tag

    }

    public enum eWSModules
    {
        API,
        PRICING,
        USERS,
        DOMAINS,
        BILLING,
        SOCIAL,
        CONDITIONALACCESS,
        CATALOG,
        NOTIFICATIONS,
        REMOTETASK
    }


    public enum eEPGFormatType
    {
        Catchup,
        StartOver,
        LivePause
    }

    public enum eStreamType
    {
        HLS,
        SS,
        DASH
    }

    public enum ePlayType
    {
        MEDIA = 0,
        NPVR = 1,
        ALL = 2,
        UNKNOWN = 3,
        EPG
    }

    public enum eOTTAssetTypes
    {
        None = 0,
        Series = 1
    }

    [XmlType("eAssetTypes", Namespace = "http://api.tvinci.com/schemas/eAssetTypes1")]
    public enum eAssetTypes
    {
        UNKNOWN = -1,
        EPG = 0,
        NPVR = 1,
        MEDIA = 2
    }

    public enum eAssetImageType
    {
        Media = 0,
        Channel = 1,
        Category = 2,
        DefaultPic = 3,
        LogoPic = 4
    }

    [Serializable]
    public enum NPVRSearchBy
    {
        [EnumMember]
        Other = 0,
        [EnumMember]
        ByRecordingID = 1,
        [EnumMember]
        ByStartDate = 2,
        [EnumMember]
        ByRecordingStatus = 3
    }

    [Serializable]
    public enum RecordingStatus
    {
        [EnumMember]
        Completed = 0,
        [EnumMember]
        Ongoing = 1,
        [EnumMember]
        Scheduled = 2,
        [EnumMember]
        Cancelled = 3
    }

    [Serializable]
    public enum EpgChannelType
    {
        [EnumMember]
        DTT = 1,
        [EnumMember]
        OTT = 2,
        [EnumMember]
        BOTH = 3
    }

    [Serializable]
    public enum TagTypeFlag
    {
        [EnumMember]
        TimeShifted = 1
    }

    [Serializable]
    public enum StatusObjectCode
    {
        [EnumMember]
        OK = 0,
        Error = 1,
        Fail = 2,
        Unkown = 3
    }

    [Serializable]
    public enum ComparisonOperator
    {
        Equals,
        NotEquals,
        Contains,
        NotContains,
        GreaterThanOrEqual,
        GreaterThan,
        LessThanOrEqual,
        LessThan,
        WordStartsWith,
        In,
        NotIn,
        Prefix
    }

    [Serializable]
    public enum eWatchStatus
    {
        [EnumMember]
        Undefined,

        [EnumMember]
        Progress,

        [EnumMember]
        Done,

        [EnumMember]
        All
    }

    [Serializable]
    public enum eTransactionState
    {
        [EnumMember]
        OK = 0,

        [EnumMember]
        Pending = 1,

        [EnumMember]
        Failed = 2
    }

    [Serializable]
    public enum eState
    {
        [EnumMember]
        OK = 0,

        [EnumMember]
        Pending = 1,

        [EnumMember]
        Failed = 2
    }

    [Serializable]
    public enum eTableStatus
    {
        [EnumMember]
        Pending = 0,

        [EnumMember]
        OK = 1,

        [EnumMember]
        Failed = 2
    }

    [Serializable]
    public enum eHouseholdPaymentGatewaySelectedBy
    {
        [EnumMember]
        None,

        [EnumMember]
        Account,

        [EnumMember]
        Household
    }
    public enum MediaPlayActions
    {
        PLAY = 1,
        STOP = 2,
        PAUSE = 3,
        FIRST_PLAY = 4,
        SWOOSH = 5,
        FULL_SCREEN = 6,
        SEND_TO_FRIEND = 7,
        LOAD = 8,
        FULL_SCREEN_EXIT = 9,
        FINISH = 10,
        BITRATE_CHANGE = 40,
        ERROR = 18,
        NONE = 99
    }

    public enum eBulkExportExportType
    {
        Full = 1,
        Incremental = 2
    }

    public enum eBulkExportDataType
    {
        VOD = 1,
        EPG = 2,
        Users = 3
    }

    public enum eMediaType
    {
        VOD = 0,
        EPG = 1
    }

    [Serializable]
    public enum ePersonalFilter
    {
        ParentalRules,
        GeoBlockRules,
        EntitledAssets
    }

    /// <summary>
    /// enum for Remote tasks: setup tasks are one-time tasks that we call when setting an environment
    /// </summary>
    [Serializable]
    public enum eSetupTask
    {
        BuildIPToCountry,
        InitializeFreeItemUpdateQueue,
        NotificationCleanupIteration,
        RecordingsCleanup,
        MigrateStatistics,
        InsertExpiredRecordingsTasks,
        RecordingScheduledTasks
    }


    [Serializable]
    public enum eOSSAdapterState
    {
        OK = 0,
        NoConfigurationForHousehold = 1
    }

    public enum ePermissionItemType
    {
        Action = 1,
        Parameter = 2
    }

    public enum ePermissionType
    {
        Normal = 1,
        Group = 2
    }

    public enum MetasEnum
    {
        META1_STR = 1,
        META2_STR = 2,
        META3_STR = 3,
        META4_STR = 4,
        META5_STR = 5,
        META6_STR = 6,
        META7_STR = 7,
        META8_STR = 8,
        META9_STR = 9,
        META10_STR = 10,
        META11_STR = 11,
        META12_STR = 12,
        META13_STR = 13,
        META14_STR = 14,
        META15_STR = 15,
        META16_STR = 16,
        META17_STR = 17,
        META18_STR = 18,
        META19_STR = 19,
        META20_STR = 20,
        META1_DOUBLE = 21,
        META2_DOUBLE = 22,
        META3_DOUBLE = 23,
        META4_DOUBLE = 24,
        META5_DOUBLE = 25,
        META6_DOUBLE = 26,
        META7_DOUBLE = 27,
        META8_DOUBLE = 28,
        META9_DOUBLE = 29,
        META10_DOUBLE = 30,
        META1_BOOL = 31,
        META2_BOOL = 32,
        META3_BOOL = 33,
        META4_BOOL = 34,
        META5_BOOL = 35,
        META6_BOOL = 36,
        META7_BOOL = 37,
        META8_BOOL = 38,
        META9_BOOL = 39,
        META10_BOOL = 40
    }

    public enum eDbActionType
    {
        Delete,
        Add
    }

    public enum eAnnouncementRecipientsType
    {
        All = 0,
        LoggedIn = 1,
        Guests = 2,
        Other = 3
    }

    public enum eMessageType
    {
        Push = 0,
        Mail = 1,
        Inbox = 2
    }

    public enum eUserMessageAction
    {
        Login = 0,
        Logout = 1,
        AnonymousPushRegistration = 2,
        IdentifyPushRegistration = 3,
        DeleteUser = 4,
        ChangeUsers = 5,
        EnableUserNotifications = 6,
        DisableUserNotifications = 7
    }

    public enum eAnnouncementStatus
    {
        [EnumMember]
        NotSent = 0,
        [EnumMember]
        Sending = 1,
        [EnumMember]
        Sent = 2,
        [EnumMember]
        Aborted = 3
    }

    public enum eIngestAction
    {
        Insert = 0,
        Update = 1,
        Delete = 2
    }

    public enum AdapterStatus
    {
        OK = 0,
        Error = 1,
        SignatureMismatch = 2,
        NoConfigurationFound = 3
    }

    public enum eFollowSeriesPlaceHolders
    {
        MediaName = 0,
        SeriesName = 1,
        CatalaogStartDate = 2,
        StartDate = 3,
        MediaId = 4,
    }
    public enum TstvRecordingStatus
    {
        OK = 0,
        Failed = 1,
        Scheduled = 2,
        Recording = 3,
        Recorded = 4,
        Canceled = 5,
        Deleted = 6,
        LifeTimePeriodExpired = 7,
        SeriesCancel = 8,
        SeriesDelete = 9
    }

    public enum DomainRecordingStatus
    {        
        None = 0,
        OK = 1,
        Canceled = 2,        
        Deleted = 3,
        DeletedBySystem = 4,
        SeriesCancel = 5,
        SeriesDelete = 6
    }

    public enum RecordingInternalStatus
    {
        OK = 0,
        Failed = 1,
        Canceled = 2,
        Waiting = 3,
        Deleted = 4
    }

    public enum eRecordingTask
    {
        GetStatusAfterProgramEnded = 1,
        Record = 2,
        UpdateRecording = 3,
        DistributeRecording = 4
    }

    public enum eSeriesRecordingTask
    {
        FirstFollower = 1,
        CompleteRecordings = 2
    }

    public enum RecordingType
    {
        Single = 0,
        Season = 1,
        Series = 2
    }

    public enum CDNProviderFailReason
    {
        Success = 0,
        internalProviderError = 1,
        BadRequest = 2,
        Unauthorized = 3,
        NoResponseFromProvider = 4
    }

    public enum eMessageCategory
    {
        SystemAnnouncement = 0,
        Followed = 1
    }

    public enum eMessageState
    {
        Unread = 0,
        Read = 1,
        Trashed = 2
    }

    public enum CdnAdapterActionType
    {
        Catchup = 0,
        StartOver = 1,
        LivePause = 2,
    }

    public enum eTopicAutomaticIssueNotification
    {
        Default = 0,
        Yes = 1,
        No = 2
    }

    public enum BulkExportTaskOrderBy
    {
        CreateDateAsc = 0,
        CreateDateDesc = 1
    }

    public enum EntitlementOrderBy
    {
        PurchaseDateAsc = 0,
        PurchaseDateDesc = 1
    }

    public enum GenericRuleOrderBy
    {
        NameAsc = 0,
        NameDesc = 1
    }

    public enum ScheduledTaskType
    {
        recordingsLifetime = 0,
        recordingsScheduledTasks = 1,
        recordingsCleanup = 2
    }
}
