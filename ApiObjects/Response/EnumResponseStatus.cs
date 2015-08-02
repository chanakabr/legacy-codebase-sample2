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
        ReasonUnknown = 3011,
        ChargeStatusUnknown = 3015,
        ContentIDMissing = 3016,   ///
        NoMediaRelatedToFile = 3017,
        NoContentID = 3018, /// 
        NoProductID = 3019,
        CouponNotValid = 3020,
        PPVPurchased = 3021,
        Free = 3022,
        ForPurchaseSubscriptionOnly = 3023,
        SubscriptionPurchased = 3024,
        NotForPurchase = 3025,
        Fail = 3026,

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
        IncorrectPrice = 6000,
        UnKnownPPVModule = 6001,
        ExpiredCard = 6002,
        CellularPermissionsError = 6003,
        UnKnownBillingProvider = 6004,
        PaymentGateWayIDMissing = 6005,
        PaymentGateWayParamsMissing = 6006,
        PaymentGateWayNotSetForHousehold = 6007,
        PaymentGateWayNotExist = 6008,
        PaymentGateWayChargeIdMissing = 6009,
        NoConfigurationFound = 6011,
        AdapterAppFailure = 6012,
        SignatureMismatch = 6013,
        ErrorSavingPaymentGatewayTransaction = 6014,
        ErrorSavingPaymentGatewayPending = 6015,
        ExternalIdntifierMissing = 6016,
        ErrorSavingPaymentGatewayHousehold = 6017,
        NoPaymentGateway = 6018,        
        PaymentGatewayNameMissing = 6020,
        PaymentGatewaySharedSecretMissing = 6021,
        PaymentGatewayAlreadyExist = 6022,        
        HouseholdAlreadySetToPaymentGateway = 6024,
        ChargeIdAlreadySetToHouseholdPaymentGateway = 6025,
        ChargeIdNotSetToHousehold = 6026,
        HouseholdNotSetToPaymentGateway = 6027,
        PaymentGatewaySelectionIsDisabled = 6028,        
        NoResponseFromPaymentGateway = 6030,
        InvalidAccount = 6031,
        InsufficientFunds = 6032,
        UnknownPaymentGatewayResponse = 6033,
        PaymentGatewayAdapterUserKnown = 6034,
        PaymentGatewayAdapterReasonUnknown = 6035,
        SignatureDoesNotMatch = 6036,
        ErrorUpdatingPendingTransaction = 6037,
        PaymentGatewayTransactionNotFound = 6038,
        PaymentGatewayTransactionIsNotPending = 6039,

        // social 7000 - 7999
        Conflict = 7000,
        MinFriendsLimitation = 7001
    }
}
