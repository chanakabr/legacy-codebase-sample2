using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/assetFile/action")]
    public class AssetFileController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// get KalturaAssetFileContext
        /// </summary>
        /// <param name="id">Asset file identifier</param>
        /// <param name="contextType">Kaltura Context Type (none = 0, recording = 1)</param>
        /// <remarks></remarks>
        [Route("getContext"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public KalturaAssetFileContext GetContext(string id, WebAPI.Models.ConditionalAccess.KalturaAssetFileContext.KalturaContextType contextType)
        {
            KalturaAssetFileContext response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "id");
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();

                response = ClientsManager.ConditionalAccessClient().GetAssetFileContext(groupId, userID, id, udid, language, contextType);

                // if no response - return not found status 
                if (response == null)
                {
                    throw new NotFoundException(NotFoundException.OBJECT_ID_NOT_FOUND, "Asset-File", id);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Redirects to play manifest
        /// </summary>
        /// <param name="assetId">Asset identifier</param>
        /// <param name="assetFileId">Asset file identifier</param>
        /// <param name="assetType">Asset type</param>
        /// <param name="contextType">Playback context type</param>
        /// <param name="ks">Kaltura session for the user, not mandatory for anonymous user</param>
        /// <param name="partnerId">Partner identifier</param>
        /// <remarks></remarks>
        // assetId/{assetId}/assetType/{assetType}/assetFileId/{assetFileId}/ks/{ks}/seekFrom/{seekFrom}
        [Route("playManifest"), HttpPost, HttpGet]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [SchemeArgument("partnerId", MinInteger = 1)]
        [SchemeArgument("assetFileId", MinInteger = 1)]
        [FailureHttpCode(System.Net.HttpStatusCode.NotFound)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.ProgramDoesntExist)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.ServiceNotAllowed)]
        [Throws(eResponseStatus.RecordingPlaybackNotAllowedForNonExistingEpgChannel)]
        [Throws(eResponseStatus.RecordingPlaybackNotAllowedForNotEntitledEpgChannel)]
        [Throws(eResponseStatus.ConcurrencyLimitation)]
        [Throws(eResponseStatus.MediaConcurrencyLimitation)]
        [Throws(eResponseStatus.DeviceTypeNotAllowed)]
        [Throws(eResponseStatus.NoFilesFound)]
        [Throws(eResponseStatus.NotEntitled)]
        public void PlayManifest(int partnerId, string assetId, KalturaAssetType assetType, long assetFileId, KalturaPlaybackContextType contextType, string ks = null)
        {
            if ((assetType == KalturaAssetType.epg && (contextType != KalturaPlaybackContextType.CATCHUP && contextType != KalturaPlaybackContextType.START_OVER)) ||
                (assetType == KalturaAssetType.media && (contextType != KalturaPlaybackContextType.TRAILER && contextType != KalturaPlaybackContextType.PLAYBACK)) ||
                (assetType == KalturaAssetType.recording && contextType != KalturaPlaybackContextType.PLAYBACK))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "assetType", "contextType");
            }

            KS ksObject = null;

            if (!string.IsNullOrEmpty(ks))
            {
                ksObject = KS.ParseKS(ks);

                if (!ksObject.IsValid)
                {
                    throw new UnauthorizedException(UnauthorizedException.KS_EXPIRED);
                }

                if (partnerId != ksObject.GroupId)
                {
                    throw new UnauthorizedException(UnauthorizedException.PARTNER_INVALID);
                }
            }

            string response = null;

            try
            {
                string userId = ksObject != null ? ksObject.UserId : "0";
                string udid = ksObject != null ? KSUtils.ExtractKSPayload(ksObject).UDID : string.Empty;

                response = ClientsManager.ConditionalAccessClient().GetPlayManifest(partnerId, userId, assetId, assetType, assetFileId, udid, contextType);

                if (!string.IsNullOrEmpty(response))
                {
                    HttpContext.Current.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    HttpContext.Current.Response.Redirect(response);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
    }
}