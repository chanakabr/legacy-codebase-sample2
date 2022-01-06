using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Reflection;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("streamingDevice")]
    public class StreamingDeviceController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Lists of devices that are streaming at that moment 
        /// </summary>
        /// <param name="filter">Segmentation type filter - basically empty</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        static public KalturaStreamingDeviceListResponse List(KalturaStreamingDeviceFilter filter = null)
        {
            KalturaStreamingDeviceListResponse response = null;

            if (filter == null)
            {
                filter = new KalturaStreamingDeviceFilter();
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

                // check if the user performing the action is domain master
                if (householdId == 0)
                {
                    throw new ClientException((int)eResponseStatus.UserWithNoDomain, "This user is not associated with any household.");
                }

                if (householdId > 0)
                {
                    response = filter.GetStreamingDevices(groupId, householdId);
                }
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}