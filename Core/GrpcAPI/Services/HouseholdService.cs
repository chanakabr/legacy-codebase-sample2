using System;
using APILogic.Api.Managers;
using phoenix;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiLogic.Users;
using ApiObjects;
using ApiObjects.Response;
using AutoMapper;
using Core.Users;
using Google.Protobuf.Collections;
using GrpcAPI.Utils;
using Microsoft.Extensions.Logging;
using Phx.Lib.Log;
using RolePermissions = ApiObjects.RolePermissions;
using Status = phoenix.Status;

namespace GrpcAPI.Services
{
    public interface IHouseholdService
    {
        ValidateUserResponse ValidateUser(ValidateUserRequest request);
        GetDomainDataResponse GetDomainData(GetDomainDataRequest request);

        bool IsPermittedPermission(IsPermittedPermissionRequest request);
        bool AllowActionInSuspendedDomain(AllowActionInSuspendedDomainRequest request);
        GetSuspensionStatusResponse GetSuspensionStatus(GetSuspensionStatusRequest request);

        GetMediaConcurrencyRulesByDomainLimitationModuleResponse
            GetMediaConcurrencyRulesByDomainLimitationModule(
                GetMediaConcurrencyRulesByDomainLimitationModuleRequest request);

        int IsDevicePlayValid(IsDevicePlayValidRequest request);
        bool IsValidDeviceFamily(IsValidDeviceFamilyRequest request);
    }

    public class HouseholdService : IHouseholdService
    {
        private const int webFamilyId = 5;
        private const string webDevice = "web site";
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod()?.DeclaringType?.ToString());

        public bool IsPermittedPermission(IsPermittedPermissionRequest request)
        {
            return RolesPermissionsManager.Instance.IsPermittedPermission(request.GroupId, request.UserId.ToString(),
                (RolePermissions) request.Role);
        }

        public bool AllowActionInSuspendedDomain(AllowActionInSuspendedDomainRequest request)
        {
            return RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(request.GroupId,
                request.UserId);
        }

        public ValidateUserResponse ValidateUser(ValidateUserRequest request)
        {
            long domainId = 0;
            try
            {
                var userStatus = Core.ConditionalAccess.Utils.ValidateUser(request.GroupId,
                    request.UserId.ToString(), ref domainId, out var user);
                var dynamicData = new MapField<string, string>();
                if (user?.m_oDynamicData?.m_sUserData != null)
                {
                    foreach (var userData in user.m_oDynamicData.m_sUserData)
                    {
                        dynamicData.Add(userData.m_sDataType, userData.m_sValue);
                    }
                }
                
                Status status = new Status();
                // Most of the cases are not interesting - focus only on those that matter
                switch (userStatus)
                {
                    case ResponseStatus.OK:
                    {
                        status.Code = (int)eResponseStatus.OK;
                        break;
                    }
                    case ResponseStatus.UserDoesNotExist:
                    {
                        status.Code = (int)eResponseStatus.UserDoesNotExist;
                        status.Message = eResponseStatus.UserDoesNotExist.ToString();
                        break;
                    }
                    //Wrong status in https://github.com/kaltura/ott-backend/blob/master/Core/Logic/ConditionalAccess/Utils.cs#L4258
                    //and all the other places in the code using it so it's very hard to change and believe that it won't
                    //any other problem
                    //TODO change the status to correct one.
                    case ResponseStatus.UserNotIndDomain:
                    {
                        if (domainId == 0)
                        {
                            status.Code = (int)eResponseStatus.UserWithNoDomain;
                            status.Message = eResponseStatus.UserWithNoDomain.ToString();
                        }
                        else
                        {
                            status.Code = (int)eResponseStatus.UserNotInDomain;
                            status.Message = "User Not In Domain";
                        }
                        break;
                    }
                    case ResponseStatus.UserWithNoDomain:
                    {
                        status.Code = (int)eResponseStatus.UserWithNoDomain;
                        status.Message = eResponseStatus.UserWithNoDomain.ToString();
                        break;
                    }
                    // Most cases will return general error
                    default:
                    {
                        status.Code = (int)eResponseStatus.Error;
                        status.Message = "Error validating user";
                        break;
                    }
                }

                return new ValidateUserResponse
                    { Status = Mapper.Map<Status>(status), DomainId = domainId, DynamicData = { dynamicData } };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping ValidateUser GRPC service {e.Message}");
                return null;
            }
        }

        public GetDomainDataResponse GetDomainData(GetDomainDataRequest request)
        {
            try
            {
                var domainId = request.DomainId;
                Core.ConditionalAccess.Utils.ValidateDomain(request.GroupId,
                    (int) domainId, out var domain);

                if (domain != null)
                {
                    var deviceFamilies = GetDeviceFamilies(domain.m_deviceFamilies);
                    return new GetDomainDataResponse()
                    {
                        DomainId = domainId,
                        DomainConcurrency = domain.m_oLimitationsManager?.Concurrency ?? 0,
                        DlmId = domain.m_nLimit,
                        DeviceFamilies =
                        {
                            deviceFamilies
                        }
                    };
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetDomainData GRPC service {e.Message}");
            }

            return null;
        }

        public int IsDevicePlayValid(IsDevicePlayValidRequest request)
        {
            try
            {
                Core.Users.BaseDomain d = null;
                Core.Users.Utils.GetBaseImpl(ref d, request.GroupId);
                if (d != null)
                {
                    var domain = d.GetDomainByUser(request.GroupId, request.UserId.ToString());
                    d.IsDevicePlayValid(request.Udid, domain, out var deviceFamilyId);
                    return deviceFamilyId;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error in IsDevicePlayValid GRPC service {e.Message}");
            }

            return -1;
        }

        public GetSuspensionStatusResponse GetSuspensionStatus(GetSuspensionStatusRequest request)
        {
            try
            {
                var status =
                    RolesPermissionsManager.GetSuspentionStatus(request.GroupId, (int) request.DomainId);
                return new GetSuspensionStatusResponse()
                {
                    Status = Mapper.Map<Status>(status)
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetSuspensionStatus GRPC service {e.Message}");
                return null;
            }
        }

        public GetMediaConcurrencyRulesByDomainLimitationModuleResponse
            GetMediaConcurrencyRulesByDomainLimitationModule(
                GetMediaConcurrencyRulesByDomainLimitationModuleRequest request)
        {
            List<int> limitationModulesRules = new List<int>();
            if (request.GroupId != 0 && request.DlmId != 0)
            {
                limitationModulesRules =
                    Core.Api.Module.GetMediaConcurrencyRulesByDomainLimitionModule(
                        request.GroupId, request.DlmId);
            }

            return new GetMediaConcurrencyRulesByDomainLimitationModuleResponse()
            {
                Ids = {limitationModulesRules}
            };
        }

        //TODO remove probably not in use
        public bool IsValidDeviceFamily(IsValidDeviceFamilyRequest request)
        {
            var deviceInfoResponse = Core.Domains.Module.Instance.GetDeviceInfo(request.GroupId, request.Udid, true);
            var familyId = deviceInfoResponse?.m_oDevice == null || deviceInfoResponse.m_oDevice.m_deviceFamilyID == 0
                ? default
                : deviceInfoResponse.m_oDevice.m_deviceFamilyID;

            return request.DeviceFamilyIds.Count == 0 || request.DeviceFamilyIds.Contains(familyId);
        }

        //helper function for web family add empty or "web site" devices
        private static List<deviceFamily> GetDeviceFamilies(List<DeviceContainer> deviceFamilies)
        {
            var webFamilyExist = false;
            var response = new List<deviceFamily>();
            if (deviceFamilies == null || deviceFamilies.Count == 0)
            {
                return response;
            }
            foreach (var deviceFamily in deviceFamilies)
            {
                if (deviceFamily.m_deviceFamilyID == webFamilyId)
                {
                    webFamilyExist = true;
                    response.Add(new deviceFamily
                    {
                        Concurrency = deviceFamily.m_deviceConcurrentLimit, FamilyId = deviceFamily.m_deviceFamilyID,
                        Udid =
                        {
                            deviceFamily.DeviceInstances.Where(z => z.m_deviceFamilyID == deviceFamily.m_deviceFamilyID)
                                .Select(y => y.m_deviceUDID).Union(new List<string> {string.Empty, webDevice})
                        }
                    });
                }
                else
                {
                    response.Add(new deviceFamily
                    {
                        Concurrency = deviceFamily.m_deviceConcurrentLimit,
                        FamilyId = deviceFamily.m_deviceFamilyID,
                        Udid =
                        {
                            deviceFamily.DeviceInstances.Where(z =>
                                    z.m_deviceFamilyID == deviceFamily.m_deviceFamilyID)
                                .Select(y => y.m_deviceUDID)
                        }
                    });
                }
            }

            if (!webFamilyExist)
            {
                response.Add(new deviceFamily
                    {Concurrency = 0, FamilyId = webFamilyId, Udid = {new List<string> {string.Empty, webDevice}}});
            }

            return response;
        }
    }
}