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
                    // pass query string params
                    if (response.ToLower().Contains("playmanifest"))
                    {
                        if (HttpContext.Current.Request.QueryString.Count > 0)
                        {
                            string dynamicQueryStringParamsConfiguration = TCMClient.Settings.Instance.GetValue<string>("PlayManifestDynamicQueryStringParamsNames");

                            // old fix for passing query string params - not using dynamic configuration
                            string[] dynamicQueryStringParamsNames;
                            if (string.IsNullOrEmpty(dynamicQueryStringParamsConfiguration))
                            {
                                dynamicQueryStringParamsNames = new string[] { "clientTag", "playSessionId" };
                            }
                            else
                            {
                                dynamicQueryStringParamsNames = dynamicQueryStringParamsConfiguration.Split(',');

                                foreach (var dynamicParam in dynamicQueryStringParamsNames)
                                {
                                    if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString[dynamicParam]))
                                    {
                                        response = string.Format("{0}{1}{2}={3}", response, response.Contains("?") ? "&" : "?", dynamicParam, HttpContext.Current.Request.QueryString[dynamicParam]);
                                    }
                                }
                            }
                        }
                    }

                    HttpContext.Current.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                    HttpContext.Current.Response.Redirect(response);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Add a new asset file
        /// </summary>
        /// <param name="assetFile">Asset object</param>
        /// <param name="assetReferenceType">Type of asset</param>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        public KalturaAssetFile Add(KalturaAssetFile assetFile, KalturaAssetReferenceType assetReferenceType)
        {
            KalturaAssetFile response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            if (assetFile.AssetId <= 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "assetId");
            }

            if (!assetFile.TypeId.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "type");
            }

            if (string.IsNullOrEmpty(assetFile.ExternalId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalId");
            }

            try
            {
                response = ClientsManager.CatalogClient().AddAssetFile(groupId, assetFile, userId, assetReferenceType);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete an existing asset file
        /// </summary>
        /// <param name="id">Asset file identifier</param>        
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [SchemeArgument("id", MinLong = 1)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.AssetFileDoesNotExist)]
        public bool Delete(long id)
        {
            bool result = false;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                result = ClientsManager.CatalogClient().DeleteAssetFile(groupId, userId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// update an existing asset file
        /// </summary>
        /// <param name="id">Asset file identifier</param>        
        /// <param name="assetFile">Asset file object</param>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetFileDoesNotExist)]
        [Throws(eResponseStatus.AssetFileNotBelongToAsset)]
        [SchemeArgument("id", MinLong = 1)]
        public KalturaAssetFile Update(long id, KalturaAssetFile assetFile)
        {
            KalturaAssetFile response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                response = ClientsManager.CatalogClient().UpdateAssetFile(groupId, id, assetFile, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns a list of asset-file types
        /// </summary>
        /// <param name="filter">Filter</param>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetFileListResponse List(KalturaAssetFileFilter filter = null)
        {
            KalturaAssetFileListResponse response = null;

            if (filter == null)
            {
                filter = new KalturaAssetFileFilter();
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                filter.Validate();

                // call client      
                response = ClientsManager.CatalogClient().GetAssetFiles(groupId, filter.IdEqual,  filter.AssetIdEqual);

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}