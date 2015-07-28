using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Response
{
    public enum eResponseStatus
    {
        OK = 0,
        Error = 1,

        // Domain Section 1000 - 1999
        DomainAlreadyExists = 1000,
        ExceededLimit = 1001,
        DeviceTypeNotAllowed = 1002,
        DeviceNotInDomain = 1003,
        MasterEmailAlreadyExists = 1004,
        UserNotInDomain = 1005,
        DomainNotExists = 1006,
        HouseholdUserFailed = 1007,
        DomainCreatedWithoutNPVRAccount = 1008,
        DomainSuspended = 1009,
        DlmNotExist = 1010,
        WrongPasswordOrUserName = 1011,
        DomainAlreadySuspended = 1012,
        DomainAlreadyActive = 1013,
        LimitationPeriod = 1014,//
        DeviceAlreadyExists = 1015,
        DeviceExistsInOtherDomains = 1016,
        NoUsersInDomain = 1017,
        UserExistsInOtherDomains = 1018,
        DeviceNotExists = 1019,
        UserNotExistsInDomain = 1020,
        ActionUserNotMaster = 1021,
        ExceededUserLimit = 1022,
        DomainNotInitialized = 1023,
        DeviceNotConfirmed = 1024,
        RequestFailed = 1025,
        InvalidUser = 1026,
        UserNotAllowed = 1027,
        DuplicatePin = 1028,
        UserAlreadyInDomain = 1029,

        // User Section 2000 - 2999
        UserDoesNotExist = 2000,
        UserSuspended = 2001,
        GenerateNewLoginPIN = 2002,
        PinNotExists = 2003,
        PinExpired = 2004,
        NoValidPin = 2006,
        MissingSecurityParameter = 2007,
        SecretIsWrong = 2008,
        LoginViaPinNotAllowed = 2009,
        PinNotInTheRightLength = 2010,
        PinAlreadyExists = 2011,
        UserExists = 2014,
        InsideLockTime = 2015,
        UserNotActivated = 2016,
        UserAllreadyLoggedIn = 2017,
        UserDoubleLogIn = 2018,
        DeviceNotRegistered = 2019,
        NotActivated = 2020,
        ErrorOnInitUser = 2021,
        UserNotMasterApproved = 2023,
        UserWithNoDomain = 2024,

        // CAS Section 3000 - 3999
        InvalidPurchase = 3000,
        CancelationWindowPeriodExpired = 3001,
        SubscriptionNotRenewable = 3002,
        ServiceNotAllowed = 3003,
        InvalidBaseLink = 3004,
        ContentAlreadyConsumed = 3005,        
        ChargeStatusUnknown = 3006,
        ContentIDMissing = 3007,
        NoMediaRelatedToFile = 3008,
        NoContentID = 3009,
        NoProductID = 3010,
        NoValidCoupon = 3011,
        PPVPurchased = 3012,
        Free = 3013,
        ForPurchaseSubscriptionOnly = 3014,
        SubscriptionPurchased = 3015,
        NotForPurchase = 3016,
        Fail = 3017,

        //Catalog 4000 - 4999
        MediaConcurrencyLimitation = 4000,
        ConcurrencyLimitation = 4001,
        BadSearchRequest = 4002,
        IndexMissing = 4003,
        SyntaxError = 4004,
        InvalidSearchField = 4005,

        // Api 5000 - 5999
        NoPinDefined = 5001,
        PinMismatch = 5002,
        RuleNotExists = 5003,

        // Billing 6000 - 6999
        PriceNotCorrect = 6000,
        UnKnownPPVModule = 6001,
        ExpiredCard = 6002,
        CellularPermissionsError = 6003,
        UnKnownBillingProvider = 6004,
        PaymentGateWayIDMissing = 6005,
        PaymentGateWayParamsMissing = 6006,
        PaymentGateWayNotSetForHH = 6007,
        PaymentGateWayNotExist = 6008,
        PaymentGateWayChargeIdMissing = 6009,
        UnknownErrorWhenConnectingPaymentGatewayAdapter = 6010,
        NoConfigurationFound = 6011,
        AdapterAppFailure = 6012,
        SignatureMismatch = 6013,
        ErrorSavingPaymentGatewayTransaction = 6014,
        ErrorSavingPaymentGatewayPending = 6015,
        ExternalIdntifierMissing = 6016,
        //PatymentGateWay Response
        SuccessfulCharge = 6017,
        NoChargeDone = 6018,
        InsufficientFunds = 6019,
        InvalidAccount = 6020,
        AccountUnknown = 6021,
        ReasonUnknown = 6022,
        UnknownPaymentGatewayResponse = 6023,
        NoResponseFromPaymentGateway = 6024,
        UnknownErrorWhenConnectingPaymentGateway = 6025,

        // social 7000 - 7999
        Conflict = 7000,
        MinFriendsLimitation = 7001
    }
}
