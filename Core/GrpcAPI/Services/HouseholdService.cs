using System;
using APILogic.Api.Managers;
using phoenix;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiLogic.Users;
using ApiObjects.Response;
using AutoMapper;
using Core.Users;
using Google.Protobuf.Collections;
using GrpcAPI.Utils;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Phx.Lib.Log;
using RolePermissions = ApiObjects.RolePermissions;
using Status = phoenix.Status;

namespace GrpcAPI.Services
{
    public interface IHouseholdService
    {
        GetDomainDataResponse GetDomainData(GetDomainDataRequest request);

        bool IsPermittedPermission(IsPermittedPermissionRequest request);
        bool AllowActionInSuspendedDomain(AllowActionInSuspendedDomainRequest request);
        GetSuspensionStatusResponse GetSuspensionStatus(GetSuspensionStatusRequest request);

        GetMediaConcurrencyRulesByDomainLimitationModuleResponse
            GetMediaConcurrencyRulesByDomainLimitationModule(
                GetMediaConcurrencyRulesByDomainLimitationModuleRequest request);

        int IsDevicePlayValid(IsDevicePlayValidRequest request);
        bool IsValidDeviceFamily(IsValidDeviceFamilyRequest request);
        GetUserDataResponse GetUserData(GetUserDataRequest request);
    }

    public class HouseholdService : IHouseholdService
    {
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

        public GetDomainDataResponse GetDomainData(GetDomainDataRequest request)
        {
            try
            {
                long domainId = request.DomainId;
                Domain domain = null;
                ApiObjects.Response.Status status = ApiObjects.Response.Status.Error;
                if (!string.IsNullOrEmpty(request.UserId.ToString()))
                {
                    status = Core.ConditionalAccess.Utils.ValidateUserAndDomain(request.GroupId,
                        request.UserId.ToString(), ref domainId, out domain);
                }

                DomainData domainData = null;
                // in case that user is in problematic status ValidateUserAndDomain won't fetch domain data
                if (domain != null)
                {
                    domainData = new DomainData()
                    {
                        Concurrency = domain.m_oLimitationsManager?.Concurrency ?? 0,
                        DlmId = domain.m_nLimit,
                        DeviceFamilies =
                        {
                            domain.m_deviceFamilies != null
                                ? Mapper.Map<RepeatedField<deviceFamilyData>>(domain.m_deviceFamilies)
                                : new RepeatedField<deviceFamilyData>()
                        }
                    };
                }
                else
                {
                    Core.ConditionalAccess.Utils.ValidateDomain(request.GroupId,
                        (int) domainId, out domain);
                    if (domain != null)
                    {
                        domainData = new DomainData()
                        {
                            Concurrency = domain.m_oLimitationsManager?.Concurrency ?? 0,
                            DlmId = domain.m_nLimit,
                            DeviceFamilies =
                            {
                                domain.m_deviceFamilies != null
                                    ? Mapper.Map<RepeatedField<deviceFamilyData>>(domain.m_deviceFamilies)
                                    : new RepeatedField<deviceFamilyData>()
                            }
                        };
                    }
                }

                return new GetDomainDataResponse()
                {
                    DomainId = domainId,
                    Status = Mapper.Map<Status>(status),
                    DomainData = domainData
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error while mapping GetDomainData GRPC service {e.Message}");
                return null;
            }
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

        public bool IsValidDeviceFamily(IsValidDeviceFamilyRequest request)
        {
            var deviceInfoResponse = Core.Domains.Module.Instance.GetDeviceInfo(request.GroupId, request.Udid, true);
            var familyId = deviceInfoResponse?.m_oDevice == null || deviceInfoResponse.m_oDevice.m_deviceFamilyID == 0
                ? default
                : deviceInfoResponse.m_oDevice.m_deviceFamilyID;

            return request.DeviceFamilyIds.Count == 0 || request.DeviceFamilyIds.Contains(familyId);
        }
        
        public GetUserDataResponse GetUserData(GetUserDataRequest request)
        {
            var deviceInfoResponse = Core.Users.Module.GetUserData(request.GroupId, request.UserId.ToString(), string.Empty);
            var dynamicData = new MapField<string, string>();
            deviceInfoResponse.m_user?.m_oDynamicData?.m_sUserData?.ForEach(x =>
                dynamicData.Add(x.m_sDataType, x.m_sValue));
            return new GetUserDataResponse
            {
                DynamicData = {dynamicData}
            };
        }
    }
}