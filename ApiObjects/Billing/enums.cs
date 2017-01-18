using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
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
        UnKnownBillingProvider = 9,
        UserSuspended = 10
    }
    public enum InAppTransactionStatus
    { 
        Success = 0,
        InvalidJSON = 21000,
        ReceiptDataMalformed = 21002,
        AuthenticatedFailed = 21003,
        SharedSecretError = 21004,
        ServerUnavailable = 21005,
        SubscriptionExpired = 21006,
        SandboxVerficationError = 21007,
        ProductionVerficationError = 21008

    }

    public enum eBillingProvider
    {
        Unknown = 0,
        Adyen = 10,
        Dummy = 11,
        Cinepolis = 17,
        Offline = 50,
        M1 = 100,
        SmartSun = 101,
        InApp = 200,
        PaymentGateway = 1000
    }

    public enum ePaymentMethod
    {
        Unknown = 0,
        CreditCard = 1,
        SMS = 2,
        PayPal = 3,
        DebitCard = 4,
        Ideal = 5,
        Incaso = 6,
        Gift = 7,
        ChangeSubscription = 8,
        Visa = 20,
        MasterCard = 21,
        Offline = 50,
        M1 = 60,
        SmartSun = 61,
        InApp = 200
    }

    public enum CancelOrRefundRequestStatus
    {
        FetchedByAdyen = 0,
        AdyenFailedToFetch = 1,
        Authorised = 2,
        Refused = 3
    }

    public enum CancelOrRefundRequestType
    {
        Cancel = 0,
        Refund = 1,
        CancelOrRefund = 2
    }

    // make sure it corresponds with BillingItemsType which appears in ConditionalAccess's enums.
    public enum ItemType
    {
        Unknown = 0,
        PPV = 1,
        Subscription = 2,
        PrePaid = 3
    }

    /* 
 * 1. Used in cinepolis_transactions table to represent the status of the transaction
 * 2. The difference between Authorised and Accepted:
 *    a. Authorised means Cinepolis authorised the transaction. Used in Cinepolis' web purchase and one click flows.
 *    b. Accepted means Cinepolis received the transaction request, and if it is refused, they will send us refusal,
 *       otherwise, it is valid. Used *only* in Cinepolis MPP renewal process.
 *    c. Refused is refused :)
 */
    public enum CinepolisTransactionStatus : byte
    {
        Authorised = 0,
        Refused = 1,
        Accepted = 2,
        Dummy = 3

    }

    public enum CinepolisConfirmationStatus : byte
    {
        NotSentYet = 0,
        Received = 1,
        Failed = 2
    }

    public enum CinepolisMailType : byte
    {
        Purchase = 0,
        RenewalFail = 1
    }

    public enum AdyenMailType : byte
    {
        None = 0,
        PurchaseSuccess = 1,
        PurchaseFail = 2,
        PurchaseWithPreviewModuleSuccess = 3,
        PreviewModuleCORSuccess = 4
    }

    public enum PaymentGatewayAdapterStatus
    {
        OK = 0,
        Error = 1,
        SignatureMismatch = 2,
        NoConfigurationFound = 3, 
    }

    public enum PaymentGatewayAdapterTransactionState
    {
        OK = 0,
        Pending = 1,
        Failed = 2,
    }

    //public enum PaymentGatewayAdapterFailReason
    //{
    //    InsufficientFunds = 20,
    //    InvalidAccount = 21,
    //    UserUnknown = 22,
    //    ReasonUnknown = 23,
    //    UnknownPaymentGatewayResponse = 24,
    //    NoResponseFromPaymentGateway = 25,
    //    ExceededRetryLimit = 26,
    //    IllegalClientRequest = 27,
    //    Expired = 28
    //}
}