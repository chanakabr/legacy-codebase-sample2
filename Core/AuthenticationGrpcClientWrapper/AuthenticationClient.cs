using Phx.Lib.Appconfig;
using Grpc;
using GrpcClientCommon;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using OTT.Service.Authentication;

namespace AuthenticationGrpcClientWrapper
{
    public class AuthenticationClient : IAuthenticationClient
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly Authentication.AuthenticationClient _Client;
        private static AuthenticationClient _Instance;
        private static readonly object _Padlock = new object();


        /// <summary>
        /// Construct GRPC client for authentication service
        /// In case service address is not configure returns null
        /// </summary>
        public static IAuthenticationClient GetClientFromTCM()
        {
            if (_Instance == null)
            {
                lock (_Padlock)
                {
                    if (_Instance == null)
                    {
                        _Instance = new AuthenticationClient();
                    }
                }
            }

            return _Instance;
        }

        private AuthenticationClient()
        {
            _Client = new Authentication.AuthenticationClient(GrpcCommon.CreateChannel(ApplicationConfiguration.Current.MicroservicesClientConfiguration.Authentication));
        }

        public UserLoginHistory GetUserLoginHistory(int partnerId, int userId)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"GetUserLoginHistory > userId:{userId}";
                    return _Client.GetUserLoginHistory(new UserLoginHistoryRequest
                    {
                        PartnerId = partnerId,
                        UserId = userId,
                    });
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling GetUserLoginHistory Auth GRPC service", e);
                return null;
            }
        }

        public bool RecordUserSuccessfulLogin(int partnerId, int userId)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"RecordUserSuccessfulLogin > userId:{userId}";
                    _Client.RecordUserSuccessfulLogin(new RecordUserSuccessfulLoginRequest
                    {
                        PartnerId = (long) partnerId,
                        UserId = (long) userId
                    });
                }

                return true;
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling RecordSuccessfulLogin Auth GRPC service", e);
                return false;
            }
        }

        public bool RecordUserFailedLogin(int partnerId, int userId)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"RecordUserFailedLogin > userId:{userId}";
                    _Client.RecordUserFailedLogin(new RecordUserFailedLoginRequest
                    {
                        PartnerId = (long) partnerId,
                        UserId = (long) userId
                    });
                }

                return true;
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling RecordFailedLogin Auth GRPC service", e);
                return false;
            }
        }

        public bool ResetUserFailedLoginCount(int partnerId, int userId)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"ResetUserFailedLoginCount > userId:{userId}";
                    _Client.ResetUserFailedLoginCount(new ResetUserFailedLoginCountRequest
                    {
                        PartnerId = (long) partnerId,
                        UserId = (long) userId
                    });
                }

                return true;
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling ResetUserFailedLoginCount Auth GRPC service", e);
                return false;
            }
        }

        public bool RecordDeviceSuccessfulLogin(int partnerId, string udid)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"RecordDeviceSuccessfulLogin > udid:{udid}";
                    _Client.RecordDeviceSuccessfulLogin(new RecordDeviceSuccessfulLoginRequest
                    {
                        PartnerId = (long) partnerId,
                        UDID = udid
                    });
                }

                return true;
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling RecordDeviceFailedLogin Auth GRPC service", e);
                return false;
            }
        }

        public bool DeleteDomainDeviceUsageDate(int partnerId, string udid)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"DeleteDomainDeviceUsageDate > udid:{udid}";
                    _Client.DeleteDeviceLoginHistory(new DeleteDeviceLoginHistoryRequest()
                    {
                        PartnerId = (long) partnerId,
                        UDID = udid
                    });
                }

                return true;
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling DeleteDomainDeviceUsageDate Auth GRPC service", e);
                return false;
            }
        }

        public string GenerateRefreshToken(int partnerId, string ks, long expirationSeconds)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"GenerateRefreshToken > ks:{ks}";
                    var refreshTokenInfo = _Client.GenerateRefreshToken(new GenerateRefreshTokenRequest
                    {
                        PartnerId = partnerId,
                        Ks = ks,
                        ExpirationSeconds = expirationSeconds,
                    });

                    return refreshTokenInfo.RefreshToken;
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling GenerateRefreshToken Auth GRPC service", e);
                return null;
            }
        }

        public IEnumerable<DeviceLoginRecord> ListDevicesLoginHistory(int partnerId, IEnumerable<string> udids)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"RecordDeviceFailedLogin > udids:{string.Join(",", udids)}";
                    var request = new ListDevicesLoginHistoryRequest();
                    request.PartnerId = partnerId;
                    request.UDIDs.AddRange(udids);
                    var response = _Client.ListDevicesLoginHistory(request);
                    return response.DevicesLoginRecord.ToList();
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling RecordDeviceFailedLogin Auth GRPC service", e);
                return Enumerable.Empty<DeviceLoginRecord>();
            }
        }

        public ListSSOAdapterProfilesResponse ListSSOAdapterProfiles(long partnerId)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"ListSSOAdapterProfiles > partnerId:{partnerId}";
                    return _Client.ListSSOAdapterProfiles(new ListSSOAdapterProfilesRequest
                    {
                        PartnerId = partnerId,
                    });
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling ListSSOAdapterProfiles Auth GRPC service", e);
                return null;
            }
        }

        public bool ValidateKs(string ks, long ksPartnerId)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, ksPartnerId.ToString()))
                {
                    mon.Table = $"ValidateKs > Ks:{ks}";
                    //Grpc call to validate ks                    
                    return _Client.GetKSRevocationStatus(new KSRevocationStatusRequest {KS = ks, PartnerId = ksPartnerId}).KSValid;
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling ValidateKS GRPC service", e);
                return false;
            }
        }


        public bool RevokeUserSession(long partnerId, long userId)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"RevokeUserSession > PartnerId:{partnerId}";
                    _Client.RevokeUserSession(new RevokeUserSessionRequest
                    {
                        PartnerId = partnerId,
                        UserId = userId
                    });
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling RevokeUserSession GRPC service", e);
                return false;
            }

            return true;
        }

        public bool RevokeUserDeviceSession(long partnerId, long userId, string udid)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"RevokeUserDeviceSession > PartnerId:{partnerId}";
                    _Client.RevokeUserDeviceSession(new RevokeUserDeviceSessionRequest
                    {
                        PartnerId = partnerId,
                        UserId = userId,
                        Udid = udid
                    });
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling RevokeUserDeviceSession GRPC service", e);
                return false;
            }

            return true;
        }


        public string GenerateDeviceLoginPin(int partnerId, string udid, long brandId)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"GenerateDeviceLoginPin > partnerId:{partnerId}";
                    var grpcResponse = _Client.GenerateDeviceLoginPin(new GenerateDeviceLoginPinRequest()
                    {
                        PartnerId = partnerId,
                        BrandId = brandId,
                        UDID = udid
                    });

                    return grpcResponse?.Pin;
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling GenerateDeviceLoginPin Auth GRPC service", e);
                return null;
            }
        }

        public LoginResponse DeviceLoginWithPin(int partnerId, string udid, string pin)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"DeviceLoginWithPin > partnerId:{partnerId}";
                    var grpcResponse = _Client.DeviceLoginWithPin(new DeviceLoginWithPinRequest()
                    {
                        PartnerId = partnerId,
                        UDID = udid,
                        Pin = pin,
                    });

                    return grpcResponse;
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling DeviceLoginWithPin Auth GRPC service", e);
                return null;
            }
        }

        public string GetDeviceLoginPin(int partnerId, string pin)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"GetDeviceLoginPin > partnerId:{partnerId}";
                    var grpcResponse = _Client.GetDeviceLoginPin(new GetDeviceLoginPinRequest()
                    {
                        PartnerId = partnerId,
                        Pin = pin,
                    });

                    return grpcResponse?.UDID;
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling GetDeviceLoginPin Auth GRPC service", e);
                return null;
            }
        }

        public GetSessionCharacteristicsResponse GetSessionCharacteristics(int partnerId, string sessionCharacteristicsId)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString()))
                {
                    mon.Table = $"GetSessionCharacteristics > partnerId:{partnerId}";
                    var grpcResponse = _Client.GetSessionCharacteristics(new GetSessionCharacteristicsRequest
                    {
                        PartnerId = partnerId,
                        Id = sessionCharacteristicsId,
                    });

                    return grpcResponse;
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling GetDeviceLoginPin Auth GRPC service", e);
                return null;
            }
        }
    }
}