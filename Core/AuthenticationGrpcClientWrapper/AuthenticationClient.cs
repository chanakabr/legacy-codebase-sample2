using ConfigurationManager;
using Grpc;
using GrpcClientCommon;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using OTT.Service.Authentication;

namespace AuthenticationGrpcClientWrapper
{
    public class AuthenticationClient
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly Authentication.AuthenticationClient _Client;
        private static AuthenticationClient _Instance;
        private static readonly object _Padlock = new object();


        /// <summary>
        /// Construct GRPC client for authentication service
        /// In case service address is not configure returns null
        /// </summary>
        public static AuthenticationClient GetClientFromTCM()
        {
            if (_Instance == null)
            {
                lock (_Padlock)
                {
                    if (_Instance == null)
                    {
                        var address = ApplicationConfiguration.Current.MicroservicesClientConfiguration.Authentication.Address.Value;
                        var certFilePath = ApplicationConfiguration.Current.MicroservicesClientConfiguration.Authentication.CertFilePath.Value;
                        _Instance = new AuthenticationClient(address, certFilePath);
                    }
                }
            }

            return _Instance;
        }

        private AuthenticationClient(string address, string certFilePath)
        {
            _Client = new Authentication.AuthenticationClient(GrpcCommon.CreateChannel(address, certFilePath));
        }

        public UserLoginHistory GetUserLoginHistory(int partnerId, int userId)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString(), "GetUserLoginHistory"))
                {
                    mon.Table = $"userId:{userId}";
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
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString(),
                    "RecordUserSuccessfulLogin"))
                {
                    mon.Table = $"userId:{userId}";
                    _Client.RecordUserSuccessfulLogin(new RecordUserSuccessfulLoginRequest
                    {
                        PartnerId = (long)partnerId,
                        UserId = (long)userId
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
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString(), "RecordUserFailedLogin"))
                {
                    mon.Table = $"userId:{userId}";
                    _Client.RecordUserFailedLogin(new RecordUserFailedLoginRequest
                    {
                        PartnerId = (long)partnerId,
                        UserId = (long)userId
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
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString(), "ResetUserFailedLoginCount"))
                {
                    mon.Table = $"userId:{userId}";
                    _Client.ResetUserFailedLoginCount(new ResetUserFailedLoginCountRequest
                    {
                        PartnerId = (long)partnerId,
                        UserId = (long)userId
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
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString(), "RecordDeviceSuccessfulLogin"))
                {
                    mon.Table = $"udid:{udid}";
                    _Client.RecordDeviceSuccessfulLogin(new RecordDeviceSuccessfulLoginRequest
                    {
                        PartnerId = (long)partnerId,
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
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString(), "DeleteDomainDeviceUsageDate"))
                {
                    mon.Table = $"udid:{udid}";
                    _Client.DeleteDeviceLoginHistory(new DeleteDeviceLoginHistoryRequest()
                    {
                        PartnerId = (long)partnerId,
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
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString(), "GenerateRefreshToken"))
                {
                    mon.Table = $"ks:{ks}";
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
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString(), "RecordDeviceFailedLogin"))
                {
                    mon.Table = $"udids:{string.Join(",", udids)}";
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
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString(), "ListSSOAdapterProfiles"))
                {
                    mon.Table = $"partnerId:{partnerId}";
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
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, ksPartnerId.ToString(), "ValidateKs"))
                {
                    mon.Table = $"Ks:{ks}";
                    //Grpc call to validate ks                    
                    return _Client.GetKSRevocationStatus(new KSRevocationStatusRequest{ KS = ks, PartnerId = ksPartnerId }).KSValid;
                }
            }
            catch (Exception e)
            {
                _Logger.Error("Error while calling ValidateKS GRPC service", e);
                //todo gil when migrate should change to false
                return true;
            }
        }

        public string GenerateDeviceLoginPin(int partnerId, string udid, long brandId)
        {
            try
            {
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString(), "GenerateDeviceLoginPin"))
                {
                    mon.Table = $"partnerId:{partnerId}";
                    var grpcResponse = _Client.GenerateDevicePin(new GenerateDevicePinRequest()
                    {
                        PartnerId = partnerId,
                        BrandId = brandId,
                        UDID = udid
                    });

                    return grpcResponse?.Pin_;
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
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString(), "DeviceLoginWithPin"))
                {
                    mon.Table = $"partnerId:{partnerId}";
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
                using (var mon = new KMonitor(Events.eEvent.EVENT_GRPC, partnerId.ToString(), "GetDeviceLoginPin"))
                {
                    mon.Table = $"partnerId:{partnerId}";
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
    }
}