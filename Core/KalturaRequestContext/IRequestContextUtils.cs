namespace KalturaRequestContext
{
    public interface IRequestContextUtils
    {
        string GetRequestId();
        long? GetUserId();
        long GetOriginalUserId();
        bool IsPartnerRequest();
        string GetUdid();
        string GetUserIp();
        void SetIsPartnerRequest();
        bool IsImpersonateRequest();
        int? GetRegionId();
        void SetRegionId(int regionId);
        void RemoveRegionId();
    }
}