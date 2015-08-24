using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Domains;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/householdDevice/action")]
    public class HouseholdDeviceController : ApiController
    {
        /// <summary>
        /// Removes a device from household
        /// </summary>        
        /// <param name="household_id">Household identifier</param>
        /// <param name="udid">device UDID</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Device not in Household = 1003,  Household suspended = 1009, Limitation period = 1014</remarks>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(string udid)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // call client
                return ClientsManager.DomainsClient().RemoveDeviceFromDomain(groupId, (int)HouseholdUtils.getHouseholdIDByKS(groupId), udid);
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
        /// <param name="device_name">Device name</param>
        /// <param name="pin">Pin code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Exceeded limit = 1001, Duplicate pin = 1028, Device not exists = 1019</remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaDevice Add(string device_name, string pin)
        {
            KalturaDevice device = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "pin cannot be empty");
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;

                // call client
                device = ClientsManager.DomainsClient().RegisterDeviceByPin(groupId, (int)HouseholdUtils.getHouseholdIDByKS(groupId), device_name, pin);
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
        /// <param name="udid">Device UDID</param>
        /// <returns></returns>
        [Route("getStatus"), HttpPost]
        [ApiAuthorize]
        public KalturaDeviceRegistrationStatusHolder GetStatus(string udid)
        {
            KalturaDeviceRegistrationStatus status = KalturaDeviceRegistrationStatus.not_registered;

            int groupId = KS.GetFromRequest().GroupId;            

            if (string.IsNullOrEmpty(udid))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "udid cannot be empty");
            }

            try
            {
                // call client
                status = ClientsManager.DomainsClient().GetDeviceRegistrationStatus(groupId, (int)HouseholdUtils.getHouseholdIDByKS(groupId), udid);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return new KalturaDeviceRegistrationStatusHolder() { Status = status };
        }
    }
}