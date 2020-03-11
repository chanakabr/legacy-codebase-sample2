namespace PGAdapter.Models
{
    public enum FailReason
    {
        Ok = 0,
        InsufficientFunds = 20,
        InvalidAccount = 21,
        UserUnknown = 22,
        ReasonUnknown = 23,
        UnknownPaymentGatewayResponse = 24,
        NoResponseFromPaymentGateway = 25,
        ExceededRetryLimit = 26,
        IllegalClientRequest = 27,
        Expired = 28,
        PaymentRejected = 29,
        Cancelled = 30,
        InvalidValidation = 31,
        Refused = 32,
        CannotBeVerified = 33,
        InternalServerError = 34
    }
}