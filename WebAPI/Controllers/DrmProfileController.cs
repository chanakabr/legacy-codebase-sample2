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
    [Service("drmProfile")]
    public class DrmProfileController : IKalturaController
    {
        /// <summary>
        /// Returns all DRM adapters for partner
        /// </summary>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaDrmProfileListResponse List()
        {
            KalturaDrmProfileListResponse response = null;

            int groupId = KSManager.GetKSFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetDrmAdapters(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}