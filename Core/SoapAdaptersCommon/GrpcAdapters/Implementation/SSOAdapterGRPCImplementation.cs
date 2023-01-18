using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;

namespace SoapAdaptersCommon.GrpcAdapters.Implementation
{
    public class SSOAdapterGRPCImplementation : SSOAdapterGRPC.SSOAdapterGRPCBase
    {
        private readonly SSOAdapter.IService _SSOService;

        public SSOAdapterGRPCImplementation(SSOAdapter.IService ssoService)
        {
            _SSOService = ssoService;
        }

        public override Task<GetConfigurationResponse> GetConfiguration(GetConfigurationRequest request, ServerCallContext context)
        {
            var result = _SSOService.GetConfiguration(request.AdapterId);
            
            var implementedMethods = result.ImplementedMethods.Select(m => (SSOMethods)m).ToList();
            var extendedImplementedMethods = result.ImplementedMethodsExtend.Select(m => (SSOMethods)m).ToList();
            var allMethods = implementedMethods.Concat(extendedImplementedMethods).Distinct();
            
            var response = new GetConfigurationResponse();
            response.AdapterStatusCode = (AdapterStatusCode)result.Status;
            response.ImplementedMethods.AddRange(allMethods);
            response.SendWelcomeEmail = result.SendWelcomeEmail;
            return Task.FromResult(response);
        }

        public override Task<SetConfigurationResponse> SetConfiguration(SetConfigurationRequest request, ServerCallContext context)
        {
            var result = _SSOService.SetConfiguration(request.AdapterId, request.PartnerId, request.Configuration, request.Signature);
            var response = new SetConfigurationResponse
            {
                AdapterStatusCode = (AdapterStatusCode)result,
            };

            return Task.FromResult(response);
        }

        public override Task<UserResponse> PostGetUserData(PostGetUserDataRequest request, ServerCallContext context)
        {
            var user = MapProtoUserToSoapUser(request.UserData);
            var result = _SSOService.PostGetUserData(request.AdapterId, user, request.CustomParams, request.Signature);
            var response = MapSoapUserResponseToProtoUserResponse(result);
            return Task.FromResult(response);
        }

        public override Task<UserResponse> PostSignIn(PostSignInRequest request, ServerCallContext context)
        {
            var user = MapProtoUserToSoapUser(request.User);
            var postSignInModel = new SSOAdapter.Models.PostSignInModel
            {
                AuthenticatedUser = user,
                CustomParams = request.CustomParams,
            };
            var result = _SSOService.PostSignIn(request.AdapterId, postSignInModel, request.Signature);
            var response = MapSoapUserResponseToProtoUserResponse(result);
            return Task.FromResult(response);
        }

        public override Task<UserResponse> PostSignOut(PostSignOutRequest request, ServerCallContext context)
        {
            var authenticatedUser = MapProtoUserToSoapUser(request.AuthenticatedUser);
            var postSignInModel = new SSOAdapter.Models.PostSignOutModel
            {
                AuthenticatedUser = authenticatedUser,
                DeviceUdid = request.DeviceUdid,
                HouseholdId = request.HouseholdId,
                UserId = request.UserId,
            };

            if (request.AdapterData != null)
            {
                postSignInModel.AdapterData = request.AdapterData.Select(kv => new AdapaterCommon.Models.KeyValue { Key = kv.Key, Value = kv.Value }).ToList();
            }

            var result = _SSOService.PostSignOut(request.AdapterId, postSignInModel, request.Signature);
            var response = MapSoapUserResponseToProtoUserResponse(result);
            return Task.FromResult(response);
        }

        public override Task<UserResponse> PreGetUserData(PreGetUserDataRequest request, ServerCallContext context)
        {
            var result = _SSOService.PreGetUserData(request.AdapterId, request.UserId, request.IpAddress, request.CustomParams, request.Signature);
            var response = MapSoapUserResponseToProtoUserResponse(result);
            return Task.FromResult(response);
        }

        public override Task<PreSignInResponse> PreSignIn(PreSignInRequest request, ServerCallContext context)
        {
            var model = new SSOAdapter.Models.PreSignInModel
            {
                CustomParams = request.CustomParams,
                UserId = request.UserId,
                DeviceId = request.DeviceId,
                GroupId = (int)request.PartnerId,
                IPAddress = request.IPAddress,
                LockMin = request.LockMin,
                MaxFailCount = request.MaxFailCount,
                Password = request.Password,
                PreventDoubleLogin = request.PreventDoubleLogin,
                SessionId = request.SessionId,
                UserName = request.UserName
            };

            var result = _SSOService.PreSignIn(request.AdapterId, model, request.Signature);
            var response = new PreSignInResponse
            {
                UserId = result.UserId,
                Username = result.Username,
                AdapterStatusCode = (AdapterStatusCode)result.AdapterStatus,
                Password = result.Password,
                SSOResponseStatus = MapSoapSSOResponseToProtoSSOResponse(result.SSOResponseStatus),
            };

            if (result.Priviliges != null)
            {
                foreach (var kv in result.Priviliges)
                {
                    response.Priviliges.Add(kv.Key, kv.Value);
                }
            }

            if (result.SessionCharacteristics != null)
            {
                foreach (var sc in result.SessionCharacteristics)
                {
                    var values = new ListOfString();
                    foreach (var valuesPairs in result.SessionCharacteristics.Values)
                    {
                        values.Value.Add(valuesPairs);
                    }
                    response.SessionCharacteristics.Add(sc.Key, values);
                }
            }

            return Task.FromResult(response);
        }

        public override Task<UserResponse> PreSignOut(PreSignOutRequest request, ServerCallContext context)
        {
            var model = new SSOAdapter.Models.PreSignOutModel
            {
                DeviceUdid = request.DeviceUdid,
                UserId = request.UserId,
                HouseholdId = request.HouseholdId,
            };

            if (request.AdapterData != null)
            {
                model.AdapterData = request.AdapterData.Select(kv => new AdapaterCommon.Models.KeyValue() { Key = kv.Key, Value = kv.Value }).ToList();
            }

            var result = _SSOService.PreSignOut(request.AdapterId, model, request.Signature);
            var response = MapSoapUserResponseToProtoUserResponse(result);
            return Task.FromResult(response);
        }

        public override Task<InvokeResponse> Invoke(InvokeRequest request, ServerCallContext context)
        {
            var model = new SSOAdapter.Models.SSOAdapterProfileInvokeModel
            {
                Intent = request.Intent,
            };

            if (request.AdapterData != null)
            {
                model.AdapterData = request.AdapterData
                    .Select(kv => new AdapaterCommon.Models.KeyValue { Key = kv.Key, Value = kv.Value }).ToList();
            }

            var result = _SSOService.Invoke(request.AdapterId, model);
            var response = new InvokeResponse
            {
                AdapterStatus = (int)result.AdapterStatus,
                Code = result.Code,
                Message = result.Message,
                SSOResponseStatus = MapSoapSSOResponseToProtoSSOResponse(result.SSOResponseStatus)
            };
            if (result.AdapterData != null)
            {
                foreach (var kv in result.AdapterData)
                {
                    response.AdapterData.Add(kv.Key, kv.Value);
                }
            }

            return Task.FromResult(response);
        }

        public override Task<AdjustRegionIdResponse> AdjustRegionId(AdjustRegionIdRequest request, ServerCallContext context)
        {
            var user = MapProtoUserToSoapUser(request.OttUser);
            var result = _SSOService.AdjustRegionId(request.AdapterId, request.DafaultRegionId, user, request.UserSegments.Value.Select(us => us).ToList(), request.AdapterData);
            var response = new AdjustRegionIdResponse { RegionId = result.RegionId, AdapterStatusCode = (AdapterStatusCode)result.AdapterStatus, SSOResponseStatus = MapSoapSSOResponseToProtoSSOResponse(result.SSOResponseStatus) };
            return Task.FromResult(response);
        }

        private static User MapSoapUserToProtoUser(SSOAdapter.Models.User user)
        {
            if (user == null) { return null; }
            UserType userType = null;
            if (user.UserType != null)
            {
                userType = new UserType
                {
                    Id = user.UserType.Id,
                    Description = user.UserType.Description,
                };
            }

            var userResponse = new User
            {
                Address = user.Address,
                City = user.City,
                CountryId = user.CountryId.HasValue ? new NullableInt32 { Value = user.CountryId.Value } : null,
                Email = user.Email,
                ExternalId = user.ExternalId,
                FirstName = user.FirstName,
                HouseholdID = user.HouseholdID.HasValue ? new NullableInt32 { Value = user.HouseholdID.Value } : null,
                Id = user.Id,
                IsHouseholdMaster = user.IsHouseholdMaster.HasValue ? new NullableBool { Value = user.IsHouseholdMaster.Value } : null,
                LastName = user.LastName,
                SuspensionState = (int)user.SuspensionState,
                Username = user.Username,
                UserState = (int)user.UserState,
                UserType = userType,
                Zip = user.Zip,
            };

            if (user.DynamicData != null)
            {
                foreach (var kv in user.DynamicData)
                {
                    userResponse.DynamicData.Add(kv.Key, kv.Value);
                }
            }

            return userResponse;
        }

        private static SSOAdapter.Models.User MapProtoUserToSoapUser(User user)
        {
            if (user == null) { return null; }
            SSOAdapter.Models.UserType userType = null;
            if (user.UserType != null)
            {
                userType = new SSOAdapter.Models.UserType
                {
                    Id = user.UserType.Id,
                    Description = user.UserType.Description,
                };
            }

            var userResponse = new SSOAdapter.Models.User
            {
                Address = user.Address,
                City = user.City,
                CountryId = user.CountryId?.Value,
                DynamicData = user.DynamicData,
                Email = user.Email,
                ExternalId = user.ExternalId,
                FirstName = user.FirstName,
                HouseholdID = user.HouseholdID?.Value,
                Id = user.Id,
                IsHouseholdMaster = user.IsHouseholdMaster?.Value,
                LastName = user.LastName,
                SuspensionState = (SSOAdapter.Models.eHouseholdSuspensionState)user.SuspensionState,
                Username = user.Username,
                UserState = (SSOAdapter.Models.eUserState)user.UserState,
                UserType = userType,
                Zip = user.Zip,
            };
            return userResponse;
        }

        private static SSOResponseStatus MapSoapSSOResponseToProtoSSOResponse(SSOAdapter.Models.SSOResponseStatus resp)
        {
            if (resp == null) { return null; }
            return new SSOResponseStatus
            {
                ExternalCode = resp.ExternalCode,
                ExternalMessage = resp.ExternalMessage,
                StatusCode = (SSOResponseStatus.Types.SSOUserResponseStatus)resp.ResponseStatus,
            };
        }

        private static UserResponse MapSoapUserResponseToProtoUserResponse(SSOAdapter.Models.UserResponse result)
        {
            return new UserResponse
            {
                AdapterStatusCode = (AdapterStatusCode)result.AdapterStatus,
                User = MapSoapUserToProtoUser(result.User),
                SSOResponseStatus = MapSoapSSOResponseToProtoSSOResponse(result.SSOResponseStatus)
            };
        }
    }
}
