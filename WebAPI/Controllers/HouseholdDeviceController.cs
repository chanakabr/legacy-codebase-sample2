using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Domains;
using WebAPI.Utils;


namespace WebAPI.Controllers
{
    [RoutePrefix("_service/householdDevice/action")]
    [OldStandardAction("addOldStandard", "add")]
    [OldStandardAction("updateOldStandard", "update")]
    public class HouseholdDeviceController : ApiController
    {
        /// <summary>
        /// Removes a device from household
        /// </summary>        
        /// <param name="udid">device UDID</param>
        /// <remarks>Possible status codes: 
        /// Device not in Household = 1003,  Household suspended = 1009, Limitation period = 1014</remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.LimitationPeriod)]
        public bool Delete(string udid)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                return ClientsManager.DomainsClient().RemoveDeviceFromDomain(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }

        /// <summary>
        /// Registers a device to a household using pin code    
        /// </summary>                
        /// <param name="deviceName">Device name</param>
        /// <param name="pin">Pin code</param>
        /// <remarks>Possible status codes: 
        /// Exceeded limit = 1001, Duplicate pin = 1028, Device not exists = 1019</remarks>
        [Route("addByPin"), HttpPost]
        [ApiAuthorize]
        [OldStandard("deviceName", "device_name")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.ExceededLimit)]
        [Throws(eResponseStatus.DuplicatePin)]
        [Throws(eResponseStatus.DeviceNotExists)]
        public KalturaHouseholdDevice AddByPin(string deviceName, string pin)
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
                device = ClientsManager.DomainsClient().RegisterDeviceByPin(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), deviceName, pin);
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
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.DeviceExistsInOtherDomains)]
        [Throws(eResponseStatus.NoUsersInDomain)]
        public KalturaHouseholdDevice Add(KalturaHouseholdDevice device)
        {
            int groupId = KS.GetFromRequest().GroupId;
            int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
            string userId = KS.GetFromRequest().UserId;

            try
            {
                if (HouseholdUtils.IsUserMaster())
                {
                    device = ClientsManager.DomainsClient().AddDevice(groupId, householdId, device.Name, device.Udid, device.getBrandId());
                }
                else if (device.HouseholdId != 0)
                {
                    device = ClientsManager.DomainsClient().AddDevice(groupId, device.HouseholdId, device.Name, device.Udid, device.getBrandId());
                }
                else
                {
                    device = ClientsManager.DomainsClient().SubmitAddDeviceToDomain(groupId, householdId, userId, device.Udid, device.Name, device.getBrandId());
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
        [Route("addOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.DeviceExistsInOtherDomains)]
        [Throws(eResponseStatus.NoUsersInDomain)]
        public KalturaHousehold AddOldStandard(string device_name, int device_brand_id, string udid)
        {
            KalturaHousehold household = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                household = ClientsManager.DomainsClient().AddDeviceToDomain(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), device_name, udid, device_brand_id);
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
        /// <returns></returns><remarks>Possible status codes: 
        /// Device does not exist = 1019, Device not in household = 1003, Device exists in other household = 1016</remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [Throws(eResponseStatus.DeviceNotExists)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.DeviceExistsInOtherDomains)]
        public KalturaHouseholdDevice Get()
        {
            KalturaHouseholdDevice device = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            if (string.IsNullOrEmpty(udid))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "udid");
            }

            try
            {
                // call client
                device = ClientsManager.DomainsClient().GetDevice(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid);
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
        [Route("getStatus"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaDeviceRegistrationStatusHolder GetStatus()
        {
            KalturaDeviceRegistrationStatus status = KalturaDeviceRegistrationStatus.not_registered;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            if (string.IsNullOrEmpty(udid))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "udid");
            }

            try
            {
                // call client
                status = ClientsManager.DomainsClient().GetDeviceRegistrationStatus(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid);
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
        [Route("generatePin"), HttpPost]
        [ApiAuthorize]
        [OldStandard("brandId", "brand_id")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public KalturaDevicePin GeneratePin(string udid, int brandId)
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
        /// <remarks>Possible status codes: 
        /// Device not exists = 1019</remarks>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.DeviceNotExists)]
        public KalturaHouseholdDevice Update(string udid, KalturaHouseholdDevice device)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // check device registration status - return forbidden if device not in domain        
                var deviceRegistrationStatus = ClientsManager.DomainsClient().GetDeviceRegistrationStatus(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid);
                if (deviceRegistrationStatus != KalturaDeviceRegistrationStatus.registered)
                {
                    throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);
                }

                // call client
                return ClientsManager.DomainsClient().SetDeviceInfo(groupId, device.Name, udid);
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
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.DeviceNotExists)]
        public bool UpdateOldStandard(string device_name, string udid)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // check device registration status - return forbidden if device not in domain        
                var deviceRegistrationStatus = ClientsManager.DomainsClient().GetDeviceRegistrationStatus(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid);
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
        /// <remarks>Possible status codes: 
        /// Limitation period = 1014, Exceeded limit = 1001 </remarks>
        [Route("updateStatus"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.LimitationPeriod)]
        [Throws(eResponseStatus.ExceededLimit)]
        public bool UpdateStatus(string udid, KalturaDeviceStatus status)
        {
            int groupId = KS.GetFromRequest().GroupId;
            int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                // check device registration status - return forbidden if device not in domain        
                var deviceRegistrationStatus = ClientsManager.DomainsClient().GetDeviceRegistrationStatus(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid);
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
        /// 
        /// </summary>
        /// <returns></returns><remarks>Possible status codes: 
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.DeviceNotExists)]
        public KalturaHouseholdDeviceListResponse List(KalturaHouseholdDeviceFilter filter = null)
        {
            KalturaHouseholdDeviceListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                KalturaHousehold household = null;
                if (filter != null && filter.HouseholdIdEqual.HasValue && filter.HouseholdIdEqual.Value > 0)
                {
                    household = ClientsManager.DomainsClient().GetDomainInfo(groupId, filter.HouseholdIdEqual.Value);
                }
                else
                {
                    household = HouseholdUtils.GetHouseholdFromRequest();
                }

                if (household == null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "householdIdEqual");
                }

                // call client
                response = ClientsManager.DomainsClient().GetHouseholdDevices(groupId, household, filter.ConvertDeviceFamilyIdIn());
                                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}