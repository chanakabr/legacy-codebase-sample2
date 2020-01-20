using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("deviceFamily")]
    public class DeviceFamilyController : IKalturaController
    {

        /// <summary>
        /// Return a list of the available device families.
        /// </summary>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaDeviceFamilyListResponse List()
        {
            KalturaDeviceFamilyListResponse response = null;
            int groupId = KSManager.GetKSFromRequest().GroupId;

            try
            {
                // call client                
                response = ClientsManager.ApiClient().GetDeviceFamilyList(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}