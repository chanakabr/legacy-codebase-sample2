using System.Collections.Generic;
using OTT.Service.Authentication;

namespace AuthenticationGrpcClientWrapper
{
    public interface IAuthenticationClient
    {
        UserLoginHistory GetUserLoginHistory(int partnerId, int userId);
        bool RecordUserSuccessfulLogin(int partnerId, int userId);
        bool RecordUserFailedLogin(int partnerId, int userId);
        bool ResetUserFailedLoginCount(int partnerId, int userId);
        bool RecordDeviceSuccessfulLogin(int partnerId, string udid);
        bool DeleteDomainDeviceUsageDate(int partnerId, string udid);
        string GenerateRefreshToken(int partnerId, string ks, long expirationSeconds);
        IEnumerable<DeviceLoginRecord> ListDevicesLoginHistory(int partnerId, IEnumerable<string> udids);
        ListSSOAdapterProfilesResponse ListSSOAdapterProfiles(long partnerId);
        bool ValidateKs(string ks, long ksPartnerId);
        bool RevokeUserSession(long partnerId, long userId);
        bool RevokeUserDeviceSession(long partnerId, long userId, string udid);
        string GenerateDeviceLoginPin(int partnerId, string udid, long brandId);
        LoginResponse DeviceLoginWithPin(int partnerId, string udid, string pin);
        string GetDeviceLoginPin(int partnerId, string pin);
        GetSessionCharacteristicsResponse GetSessionCharacteristics(int partnerId, string sessionCharacteristicsId);
    }
}