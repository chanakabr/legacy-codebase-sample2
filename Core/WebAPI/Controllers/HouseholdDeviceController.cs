using ApiLogic.Users.Services;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.Roles;
using ApiObjects.User;
using KalturaRequestContext;
using System;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Users;
using WebAPI.Utils;
using WebAPI.ModelsValidators;

namespace WebAPI.Controllers
{
    [Service("householdDevice")]
    public class HouseholdDeviceController : IKalturaController
    {
        /// <summary>
        /// Removes a device from household
        /// </summary>        
        /// <param name="udid">device UDID</param>
        /// <remarks>Possible status codes: 
        /// Device not in Household = 1003,  Household suspended = 1009, Limitation period = 1014</remarks>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.LimitationPeriod)]
        [Throws(eResponseStatus.DeviceNotExists)]
        static public bool Delete(string udid)
        {
            bool res = false;
            KS ks = KS.GetFromRequest();
            int groupId = ks.GroupId;
            var householdId = HouseholdUtils.GetHouseholdIDByKS();

            try
            {
                var userRoles = RolesManager.GetRoleIds(ks);
                if (userRoles.Contains(PredefinedRoleId.OPERATOR) || userRoles.Contains(PredefinedRoleId.MANAGER) || userRoles.Contains(PredefinedRoleId.ADMINISTRATOR))
                {
                    res = ClientsManager.DomainsClient().DeleteDevice(groupId, udid, out var domainId);
                    householdId = domainId;
                }
                else
                {
                    res = ClientsManager.DomainsClient().RemoveDeviceFromDomain(groupId, (int)householdId, udid);
                }

                DeviceRemovalPolicyHandler.Instance.DeleteDomainDeviceUsageDate(udid, groupId);
                AuthorizationManager.RevokeHouseholdSessions(groupId, householdId, udid, null);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return res;
        }

        /// <summary>
        /// Registers a device to a household using pin code    
        /// </summary>                
        /// <param name="deviceName">Device name</param>
        /// <param name="pin">Pin code</param>
        /// <remarks>Possible status codes: 
        /// Exceeded limit = 1001, Duplicate pin = 1028, Device not exists = 1019</remarks>
        [Action("addByPin")]
        [ApiAuthorize]
        [OldStandardArgument("deviceName", "device_name")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.ExceededLimit)]
        [Throws(eResponseStatus.DuplicatePin)]
        [Throws(eResponseStatus.DeviceNotExists)]
        static public KalturaHouseholdDevice AddByPin(string deviceName, string pin)
        {
            KalturaHouseholdDevice device = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "pin");
            }

            try
            {
                // call client
                device = ClientsManager.DomainsClient().RegisterDeviceByPin(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(), deviceName, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return device;
        }

        /// <summary>
        /// Add device to household
        /// </summary>                
        /// <param name="device">Device</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, Household suspended = 1009, Device exists in other household = 1016 , Device already exists = 1015, No users in household = 1017</remarks>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.DeviceExistsInOtherDomains)]
        [Throws(eResponseStatus.NoUsersInDomain)]
        [Throws(eResponseStatus.DeviceTypeNotAllowed)]
        [Throws(eResponseStatus.ExternalIdAlreadyExists)]        
        [Throws(eResponseStatus.ExceededLimit)]
        public static KalturaHouseholdDevice Add(KalturaHouseholdDevice device)
        {
            device.Validate();

            int groupId = KS.GetFromRequest().GroupId;
            int householdId = (int)HouseholdUtils.GetHouseholdIDByKS();
            long userId = KS.GetFromRequest().UserId.ParseUserId();
            var contextData = new ContextData(groupId) { UserId = userId };

            try
            {
                ClientsManager.DomainsClient().ValidateDeviceReferencesData(contextData, device);

                if (HouseholdUtils.IsUserMaster())
                {
                    device = ClientsManager.DomainsClient().AddDevice(groupId, householdId, device);
                }
                else if (device.HouseholdId != 0)
                {
                    device = ClientsManager.DomainsClient().AddDevice(groupId, device.HouseholdId, device);
                }
                else
                {
                    device = ClientsManager.DomainsClient().SubmitAddDeviceToDomain(groupId, householdId, userId.ToString(), device);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return device;
        }

        /// <summary>
        /// Add device to household
        /// </summary>                
        /// <param name="device_name">Device name</param>
        /// <param name="device_brand_id">Device brand identifier</param>
        /// <param name="udid">Device UDID</param>
        /// <remarks>Possible status codes: 
        /// Household does not exist = 1006, Household suspended = 1009, Device exists in other household = 1016 , Device already exists = 1015</remarks>
        [Action("addOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("add")]
        [Obsolete]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.DeviceExistsInOtherDomains)]
        [Throws(eResponseStatus.NoUsersInDomain)]
        static public KalturaHousehold AddOldStandard(string device_name, int device_brand_id, string udid)
        {
            KalturaHousehold household = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                household = ClientsManager.DomainsClient().AddDeviceToDomain(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(), device_name, udid, device_brand_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return household;
        }

        /// <summary>
        /// Returns device registration status to the supplied household
        /// </summary>
        /// <param name="udid">device id</param>
        /// <returns></returns><remarks>Possible status codes: 
        /// Device does not exist = 1019, Device not in household = 1003, Device exists in other household = 1016</remarks>
        [Action("get")]
        [ApiAuthorize]
        [SchemeArgument("udid", RequiresPermission = true)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [Throws(eResponseStatus.DeviceNotExists)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.DeviceExistsInOtherDomains)]
        [Throws(eResponseStatus.AdapterNotExists)]
        [Throws(eResponseStatus.AdapterAppFailure)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        static public KalturaHouseholdDevice Get(string udid = null)
        {
            KalturaHouseholdDevice device = null;

            KS ks = KS.GetFromRequest();            

            int householdId = 0;
            if (udid.IsNullOrEmptyOrWhiteSpace())
            {
                udid = KSUtils.ExtractKSPayload().UDID;
                householdId = (int)HouseholdUtils.GetHouseholdIDByKS();
            }

            if (string.IsNullOrEmpty(udid))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "udid");
            }

            try
            {
                // call client
                device = ClientsManager.DomainsClient().GetDevice(ks.GroupId, householdId, udid, ks.UserId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return device;
        }

        /// <summary>
        /// Returns device registration status to the supplied household
        /// </summary>
        /// <returns></returns>
        [Action("getStatus")]
        [ApiAuthorize]
        [Obsolete]
        static public KalturaDeviceRegistrationStatusHolder GetStatus(string udid = null)
        {
            KalturaDeviceRegistrationStatus status = KalturaDeviceRegistrationStatus.not_registered;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(udid))
            {
                udid = KSUtils.ExtractKSPayload().UDID;
            }
            if (string.IsNullOrEmpty(udid))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "udid");
            }

            try
            {
                // call client
                status = ClientsManager.DomainsClient().GetDeviceRegistrationStatus(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(), udid);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return new KalturaDeviceRegistrationStatusHolder() { Status = status };
        }

        /// <summary>
        /// Generates device pin to use when adding a device to household by pin
        /// </summary>
        /// <param name="brandId">Device brand identifier</param>
        /// <param name="udid">Device UDID</param>
        /// <returns></returns>
        [Action("generatePin")]
        [ApiAuthorize]
        [OldStandardArgument("brandId", "brand_id")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public KalturaDevicePin GeneratePin(string udid, int brandId)
        {
            KalturaDevicePin devicePin = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(udid))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "udid");
            }

            try
            {
                // call client
                devicePin = ClientsManager.DomainsClient().GetPinForDevice(groupId, udid, brandId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return devicePin;
        }

        /// <summary>
        /// Update the name of the device by UDID
        /// </summary>                
        /// <param name="udid">Device UDID</param>
        /// <param name="device">Device object</param>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DeviceNotExists)]
        public static KalturaHouseholdDevice Update(string udid, KalturaHouseholdDevice device)
        {
            device.Udid = udid;
            device.Validate();

            int groupId = KS.GetFromRequest().GroupId;
            
            try
            {
                string userId = KS.GetFromRequest().UserId;
                int.TryParse(userId, out int _userId);
                var contextData = new ContextData(groupId) { UserId = _userId };

                ClientsManager.DomainsClient().ValidateDeviceReferencesData(contextData, device);

                // check device registration status - return forbidden if device not in domain        
                var deviceRegistrationStatus = ClientsManager.DomainsClient().GetDeviceRegistrationStatus(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(), udid);
                if (deviceRegistrationStatus != KalturaDeviceRegistrationStatus.registered)
                {
                    throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);
                }

                var allowNullExternalId = device.NullableProperties != null && device.NullableProperties.Contains("externalid");
                var allowNullMacAddress = device.NullableProperties != null && device.NullableProperties.Contains("macaddress");
                var allowNullDynamicData = device.NullableProperties != null && device.NullableProperties.Contains("dynamicdata");

                // call client
                return ClientsManager.DomainsClient().SetDeviceInfo(groupId, device, allowNullExternalId, allowNullMacAddress, allowNullDynamicData);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return null;
        }

        /// <summary>
        /// Update the name of the device by UDID
        /// </summary>                
        /// <param name="device_name">Device name</param>
        /// <param name="udid">Device UDID</param>
        /// <remarks>Possible status codes: 
        /// Device not exists = 1019</remarks>
        [Action("updateOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("update")]
        [Obsolete]
        [Throws(eResponseStatus.DeviceNotExists)]
        static public bool UpdateOldStandard(string device_name, string udid)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // check device registration status - return forbidden if device not in domain        
                var deviceRegistrationStatus = ClientsManager.DomainsClient().GetDeviceRegistrationStatus(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(), udid);
                if (deviceRegistrationStatus != KalturaDeviceRegistrationStatus.registered)
                {
                    throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);
                }

                // call client
                ClientsManager.DomainsClient().SetDeviceInfo(groupId, device_name, udid);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return true;
        }

        /// <summary>
        /// Update the name of the device by UDID
        /// </summary>                
        /// <param name="udid">Device UDID</param>
        /// <param name="status">Device status</param>
        [Action("updateStatus")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.LimitationPeriod)]
        [Throws(eResponseStatus.ExceededLimit)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        static public bool UpdateStatus(string udid, KalturaDeviceStatus status)
        {
            int groupId = KS.GetFromRequest().GroupId;
            int householdId = (int)HouseholdUtils.GetHouseholdIDByKS();

            try
            {
                // check device registration status - return forbidden if device not in domain        
                var deviceRegistrationStatus = ClientsManager.DomainsClient().GetDeviceRegistrationStatus(groupId, householdId, udid);
                if (deviceRegistrationStatus != KalturaDeviceRegistrationStatus.registered)
                {
                    throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);
                }

                if (status == KalturaDeviceStatus.PENDING)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "status", "KalturaDeviceStatus.PENDING");
                }

                // call client
                return ClientsManager.DomainsClient().ChangeDeviceDomainStatus(groupId, householdId, udid, status);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return false;
        }

        /// <summary>
        /// Returns the devices within the household
        /// </summary>
        /// <param name="filter">Household devices filter</param>
        [Action("list")]
        [ApiAuthorize(eKSValidation.Expiration)]
        [Throws(eResponseStatus.DomainNotExists)]
        static public KalturaHouseholdDeviceListResponse List(KalturaHouseholdDeviceFilter filter = null)
        {
            KalturaHouseholdDeviceListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                KalturaHousehold household = null;
                if (filter == null)
                {
                    filter = new KalturaHouseholdDeviceFilter();
                }

                if (filter.HouseholdIdEqual.HasValue && filter.HouseholdIdEqual.Value > 0)
                {
                    bool isPartnerRequest = RequestContextUtilsInstance.Get().IsPartnerRequest();

                    if (isPartnerRequest)  //BEO-11707
                    {
                        household = ClientsManager.DomainsClient().GetDomainInfo(groupId, filter.HouseholdIdEqual.Value);
                    }
                    else
                    {
                        var userHousehold = HouseholdUtils.GetHouseholdFromRequest();
                        if (userHousehold != null && userHousehold.Id.Value == filter.HouseholdIdEqual.Value)
                        {
                            household = userHousehold;
                        }
                        else
                        {
                            var status = new ApiObjects.Response.Status(eResponseStatus.DomainNotExists, "Household does not exist");
                            throw new ClientException(status);
                        }
                    }
                }
                else
                {
                    household = HouseholdUtils.GetHouseholdFromRequest();
                }

                if (household == null && string.IsNullOrEmpty(filter.ExternalIdEqual))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "householdIdEqual", "externalIdEqual");
                }

                // call client
                response = ClientsManager.DomainsClient().GetHouseholdDevices(groupId, household, filter.ConvertDeviceFamilyIdIn(), filter.ExternalIdEqual, filter.OrderBy);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// User sign-in via a time-expired sign-in PIN.        
        /// </summary>
        /// <param name="partnerId">Partner Identifier</param>
        /// <param name="pin">pin code</param>
        /// <param name="secret">Additional security parameter to validate the login</param>
        /// <param name="udid">Device UDID</param>
        /// <param name="extraParams">extra params</param>
        [Action("loginWithPin")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.WrongPasswordOrUserName)]
        [Throws(eResponseStatus.PinNotExists)]
        [Throws(eResponseStatus.NoValidPin)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserNotActivated)]
        [Throws(eResponseStatus.UserAllreadyLoggedIn)]
        [Throws(eResponseStatus.UserDoubleLogIn)]
        [Throws(eResponseStatus.ErrorOnInitUser)]
        [Throws(eResponseStatus.UserNotMasterApproved)]
        [Throws(eResponseStatus.DeviceNotExists)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.MasterUserNotFound)]
        static public KalturaLoginResponse LoginWithPin(int partnerId, string pin, string udid = null, SerializableDictionary<string, KalturaStringValue> extraParams = null)
        {
            KalturaOTTUser response = null;

            try
            {
                // call client
                response = ClientsManager.UsersClient().LoginWithDevicePin(partnerId, udid, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return new KalturaLoginResponse()
            {
                LoginSession = AuthorizationManager.GenerateSession(response.Id.ToString(), partnerId, false, true, response.getHouseholdID(), 
                    udid, response.GetRoleIds()),
                User = response
            };
        }

        /// <summary>
        /// Adds or updates dynamic data item for device with identifier udid. If it is needed to update several items, use a multi-request to avoid race conditions.
        /// </summary>
        /// <param name="udid">Unique identifier of device.</param>
        /// <param name="key">Key of dynamic data item. Max length of key is 125 characters.</param>
        /// <param name="value">Value of dynamic data item. Max length of value is 255 characters.</param>
        /// <returns>Added or updated dynamic data item.</returns>
        /// <remarks>Possible status codes: ServiceForbidden = 500004, ArgumentCannotBeEmpty = 50027, ArgumentMaxLengthCrossed = 500045, DeviceNotExists = 1019, ExceededMaxCapacity = 9028.</remarks>
        [Action("upsertDynamicData")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DeviceNotExists)]
        [Throws(eResponseStatus.ExceededMaxCapacity)]
        [Throws(eResponseStatus.ExceededMaxLength)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static KalturaDynamicData UpsertDynamicData(string udid, string key, KalturaStringValue value)
        {
            if (string.IsNullOrWhiteSpace(udid))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(udid));
            }
            KalturaDeviceDynamicDataValidator.Validate(key, value);

            var groupId = KS.GetFromRequest().GroupId;

            KalturaDynamicData response = null;
            try
            {
                if (!RequestContextUtilsInstance.Get().IsPartnerRequest())
                {
                    var deviceRegistrationStatus = ClientsManager.DomainsClient().GetDeviceRegistrationStatus(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(), udid);
                    if (deviceRegistrationStatus != KalturaDeviceRegistrationStatus.registered)
                    {
                        throw new UnauthorizedException(BadRequestException.SERVICE_FORBIDDEN);
                    }
                }
                
                response = ClientsManager.DomainsClient().UpsertDeviceDynamicData(groupId, udid, key, value);
            }
            catch (ClientException e)
            {
                ErrorUtils.HandleClientException(e);
            }

            return response;
        }

        /// <summary>
        /// Deletes dynamic data item with key <see cref="key"/> for device with identifier <see cref="udid"/>.
        /// </summary>
        /// <param name="udid">Unique identifier of device.</param>
        /// <param name="key">Key of dynamic data item.</param>
        /// <returns>True if dynamic data item has been successfully deleted. Otherwise see possible error codes.</returns>
        /// <remarks>Possible status codes: ServiceForbidden = 500004, ArgumentCannotBeEmpty = 50027, DeviceNotExists = 1019, ItemNotFound = 2032.</remarks>
        [Action("deleteDynamicData")]
        [ApiAuthorize]
        [Throws((eResponseStatus.DeviceNotExists))]
        [Throws(eResponseStatus.ItemNotFound)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static bool DeleteDynamicData(string udid, string key)
        {
            if (string.IsNullOrWhiteSpace(udid))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(udid));
            }

            var groupId = KS.GetFromRequest().GroupId;

            var response = false;
            try
            {
                if (!RequestContextUtilsInstance.Get().IsPartnerRequest())
                {
                    var deviceRegistrationStatus = ClientsManager.DomainsClient().GetDeviceRegistrationStatus(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(), udid);
                    if (deviceRegistrationStatus != KalturaDeviceRegistrationStatus.registered)
                    {
                        throw new UnauthorizedException(BadRequestException.SERVICE_FORBIDDEN);
                    }
                }

                response = ClientsManager.DomainsClient().DeleteDeviceDynamicData(groupId, udid, key);
            }
            catch (ClientException e)
            {
                ErrorUtils.HandleClientException(e);
            }

            return response;
        }
    }
}
