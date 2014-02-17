using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public enum PriceReason
    {
        Error = 0,
        PPVPurchased = 1,
        Free = 2,
        ForPurchaseSubscriptionOnly = 3,
        SubscriptionPurchased = 4,
        ForPurchase = 5,
        UnKnown = 6,
        SubscriptionPurchasedWrongCurrency = 7,
        PrePaidPurchased = 8,
        GeoCommerceBlocked = 9,
        EntitledToPreviewModule = 10,
        FirstDeviceLimitation = 11
    }

    public enum BillingItemsType
    {
        Unknown = 0,
        PPV = 1,
        Subscription = 2,
        PrePaid = 3,
        PrePaidExpired = 4
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
        M1 = 60
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

    public enum BillingResponseStatus
    {
        Success = 0,
        Fail = 1,
        UnKnown = 2,
        PriceNotCorrect = 3,
        UnKnownUser = 4,
        UnKnownPPVModule = 5,
        ExpiredCard = 6,
        ExternalError = 7,
        CellularPermissionsError = 8,        
        UnKnownBillingProvider = 9
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
        NoCredit = 7
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
        LivePause
    }

    public enum eStreamType
    {
        HLS,
        SS,
        DASH
    }

    public enum eBillingProvider
    {
       Unknown = 0,
       Adyen = 10,
       Dummy = 11,
       Cinepolis = 17,
       M1 = 100
    }

    public enum CinepolisConfirmationStatus : byte
    {
        NotSentYet = 0,
        Received = 1,
        Failed = 2
    }
}
