namespace KalturaRequestContext
{
    public interface IRequestContextUtils
    {
        string GetRequestId();
        long? GetPartnerId();
        long? GetUserId();
        long GetOriginalUserId();
        bool IsPartnerRequest();
        string GetUdid();
        string GetUserIp();
        bool IsImpersonateRequest();
        int? GetRegionId();
        string GetSessionCharacteristicKey();
    }
}