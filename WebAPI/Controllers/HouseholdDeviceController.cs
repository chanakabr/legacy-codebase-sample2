using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Domains;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("household_device")]
    public class HouseholdDeviceController : ApiController
    {
        /// <summary>
        /// Removes a device from household
        /// </summary>        
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="udid">device UDID</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Household suspended = 1009, No users in household = 1017, Action user not master = 1021</remarks>
        [Route("{household_id}/devices/{udid}"), HttpDelete]
        public bool Delete([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string udid)
        {
            int groupId = int.Parse(partner_id);

            try
            {
                // call client
                return ClientsManager.DomainsClient().RemoveDeviceFromDomain(groupId, household_id, udid);
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
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <param name="device_name">Device name</param>
        /// <param name="pin">Pin code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, 
        /// Exceeded limit = 1001, Duplicate pin = 1028, Device not exists = 1019</remarks>
        [Route("{household_id}/devices/pin"), HttpPost]
        public KalturaDevice Add([FromUri] string partner_id, [FromUri] int household_id, [FromUri] string device_name, [FromUri] string pin)
        {
            KalturaDevice device = null;

            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(pin))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "pin cannot be empty");
            }

            try
            {
                // call client
                device = ClientsManager.DomainsClient().RegisterDeviceByPin(groupId, household_id, device_name, pin);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return device;
        }
    }
}