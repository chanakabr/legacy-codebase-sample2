using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Reflection;
using WebAPI.ClientManagers.Client;
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
                long householdId = HouseholdUtils.GetHouseholdIDByKS();

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

        /// <summary>
        /// Reserves a concurrency slot for the given asset-device combination
        /// </summary>
        /// <param name="assetId">KalturaAsset.id - asset for which a concurrency slot is being reserved</param>
        /// <param name="fileId">KalturaMediaFile.id media file belonging to the asset for which a concurrency slot is being reserved</param>
        /// <param name="assetType">Identifies the type of asset for which the concurrency slot is being reserved</param>
        /// <returns>true if Playback Service successfully reserved a concurrency slot. false otherwise.</returns>
        [Action("bookPlaybackSession")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.ConcurrencyLimitation)]
        static public bool BookPlaybackSession(string fileId, string assetId, KalturaAssetType assetType)
        {
            bool result = false;
            
            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                var userId = KS.GetFromRequest().UserId;

                result = ClientsManager.ConditionalAccessClient().BookPlaybackSession(groupId, userId, udid, assetId, fileId, assetType);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }
    }
}