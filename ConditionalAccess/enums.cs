using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ConditionalAccess
{
    public enum PriceReason : int
    {
        PPVPurchased = 0,
        Free = 1,
        ForPurchaseSubscriptionOnly = 2,
        SubscriptionPurchased = 3,
        ForPurchase = 4,
        UnKnown = 5,
        SubscriptionPurchasedWrongCurrency = 6,
        PrePaidPurchased = 7,
        GeoCommerceBlocked = 8,
        EntitledToPreviewModule = 9,
        FirstDeviceLimitation =10,
        CollectionPurchased = 11,
        UserSuspended = 12, 
        NotForPurchase = 13,
        InvalidCurrency = 14,
        CurrencyNotDefinedOnPriceCode = 15
    }

    public enum MediaFileStatus
    {
        OK = 0,
        NotForPurchase = 1,
        ValidOnlyIfPurchase = 2
    }


    public enum BillingItemsType
    {
        Unknown = 0,
        PPV = 1,
        Subscription = 2,
        PrePaid = 3,
        PrePaidExpired = 4,
        Collection = 5
    }

    public enum PaymentMethod
    {
        Unknown = 0,
        CreditCard = 1,
        SMS = 2,
        PayPal = 3,
        DebitCard = 4,
        Ideal = 5,
        Incaso = 6,
        Gift = 7,
        Visa = 20,
        MasterCard = 21,
        InApp = 200,
        M1 = 60,
        ChangeSubscription = 8,
        Offline = 50
        // PS takes care of billing in the payment day. We still don't know the method
    }

    public enum BillingAction
    {
        Unknown = 0,
        Purchase = 1,
        RenewPayment = 2,
        RenewCancledSubscription = 3,
        CancelSubscriptionOrder = 4,
        SubscriptionDateChanged = 5
    }

    public enum UserCAStatus
    {
        Annonymus = 0,
        NeverPurchased = 1,
        ExPPV = 2,
        ExSub = 3,
        CurrentPPV = 4,
        CurrentSub = 5
    }

    public enum TransactionHistoryOrderBy
    {
        CreateDateAsc = 0,
        CreateDateDesc = 1
    }

    public enum PrePaidResponseStatus
    {
        Success = 0,
        Fail = 1,
        UnKnown = 2,
        PriceNotCorrect = 3,
        UnKnownUser = 4,
        UnKnownPPVModule = 5,
        UnKnownPPModule = 6,
        NoCredit = 7,
        UserSuspended = 8
    }

    public enum CampaignActionResult
    {
        OK,
        ERROR
    }

    public enum eEPGFormatType
    {
        Catchup,
        StartOver,
        LivePause,
        NPVR
    }

    public enum eStreamType
    {
        HLS,
        SS,
        DASH
    }

    public enum CinepolisConfirmationStatus : byte
    {
        NotSentYet = 0,
        Received = 1,
        Failed = 2
    }

    public enum eBundleType
    {
        SUBSCRIPTION = 0,
        COLLECTION = 1
	}
	
    public enum ChangeSubscriptionStatus
    {
        OK = 0,
        UserNotExists = 1,
        OldSubNotExists = 2,
        NewSubNotExits = 3,
        OldSubNotRenewable = 4,
        NewSubNotRenewable = 5,
        UserHadNewSub = 6,
        Error = 7,
        UserSuspended = 8,
    }
    
    internal enum BundleToUse : byte
    {
        All = 0,
        Subscription = 1,
        Collection = 2
    }

    public enum eLicensedLinkStatus
    {
        Unknown = 0,
        OK = 1,
        Error = 2,
        InvalidInput = 3,
        InvalidDevice = 4,
        InvalidPrice = 5,
        Concurrency = 6,
        MediaConcurrency = 7,
        InvalidBaseLink = 8,
        InvalidFileData = 9,
        UserSuspended = 10,
        ServiceNotAllowed = 11
    }

    public enum eUrlType
    {
        Static = 0,        
        Dynamic = 1
    }

    public enum NPVRStatus : byte
    {
        Unknown = 0,
        OK = 1,
        Error = 2,
        BadRequest = 3,
        InvalidUser = 4,
        InvalidAssetID = 5,
        AssetAlreadyScheduled = 6,
        AssetAlreadyCanceled = 7,
        AssetDoesNotExist = 8,
        AssetAlreadyRecorded = 9,
        QuotaExceeded = 10,
        Suspended = 11,
        ServiceNotAllowed = 12,
        NotPurchased = 13
    }

    //public enum eService
    //{
    //    Unknown = 0,
    //    CatchUp = 1,
    //    StartOver = 2,
    //    NPVR = 3,
    //    Download = 4
    //}

}
