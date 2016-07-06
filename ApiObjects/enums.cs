using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects
{
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
        Media,
        [EnumMember]
        Channel,
        [EnumMember]
        EPG
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
        Off,
        [EnumMember]
        On,
        [EnumMember]
        Update,
        [EnumMember]
        Delete,
        [EnumMember]
        Rebuild
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
        NOTIFICATION,
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
        UNKNOWN = 3
    }



    public enum eAssetTypes
    {
        UNKNOWN = -1,
        EPG = 0,
        NPVR = 1,
        MEDIA = 2
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

<<<<<<< HEAD
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
    public enum eHouseholdPaymentGatewaySelectedBy
    {
        [EnumMember]
        Account,

        [EnumMember]
        Household
=======
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
>>>>>>> origin/Maverick
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
        BuildIPToCountry
    }


    [Serializable]
    public enum eOSSAdapterState
    {
        OK = 0,
        NoConfigurationForHousehold = 1
    }
}
