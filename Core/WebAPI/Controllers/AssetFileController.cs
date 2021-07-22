using ApiObjects.Response;
using ConfigurationManager;
using KLogMonitor;
using System;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("assetFile")]
    public class AssetFileController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        /// <summary>
        /// get KalturaAssetFileContext
        /// </summary>
        /// <param name="id">Asset file identifier</param>
        /// <param name="contextType">Kaltura Context Type (none = 0, recording = 1)</param>
        /// <remarks></remarks>
        [Action("getContext")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(StatusCode.ObjectIdNotFound)]
        static public KalturaAssetFileContext GetContext(string id, KalturaContextType contextType)
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
        /// <param name="tokenizedUrl">Tokenized Url, not mandatory</param>
        /// <param name="isAltUrl">Is alternative url</param>
        /// <remarks></remarks>
        // assetId/{assetId}/assetType/{assetType}/assetFileId/{assetFileId}/ks/{ks}/seekFrom/{seekFrom}
        [Action("playManifest")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [SchemeArgument("partnerId", MinInteger = 1)]
        [SchemeArgument("assetFileId", MinInteger = 1)]
        [FailureHttpCode(HttpStatusCode.NotFound)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.ProgramDoesntExist)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UserWithNoDomain)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.ServiceNotAllowed)]
        [Throws(eResponseStatus.RecordingPlaybackNotAllowedForNonExistingEpgChannel)]
        [Throws(eResponseStatus.RecordingPlaybackNotAllowedForNotEntitledEpgChannel)]
        [Throws(eResponseStatus.ConcurrencyLimitation)]
        [Throws(eResponseStatus.MediaConcurrencyLimitation)]
        [Throws(eResponseStatus.DeviceTypeNotAllowed)]
        [Throws(eResponseStatus.NoFilesFound)]
        [Throws(eResponseStatus.NotEntitled)]
        [Throws(eResponseStatus.NotAllowed)]
        [Throws(eResponseStatus.AccountCatchUpNotEnabled)]
        [Throws(eResponseStatus.ProgramCatchUpNotEnabled)]
        [Throws(eResponseStatus.CatchUpBufferLimitation)]
        [Throws(eResponseStatus.NetworkRuleBlock)]
        [Throws(eResponseStatus.ActionBlocked)]
        static public KalturaAssetFile PlayManifest(int partnerId, string assetId, KalturaAssetType assetType, 
            long assetFileId, KalturaPlaybackContextType contextType, string ks = null, string tokenizedUrl = null, bool isAltUrl = false)
        {
            if ((assetType == KalturaAssetType.epg && (contextType != KalturaPlaybackContextType.CATCHUP && contextType != KalturaPlaybackContextType.START_OVER)) ||
                (assetType == KalturaAssetType.media && (contextType != KalturaPlaybackContextType.TRAILER && contextType != KalturaPlaybackContextType.PLAYBACK)) ||
                (assetType == KalturaAssetType.recording && contextType != KalturaPlaybackContextType.PLAYBACK))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "assetType", "contextType");
            }

            KS ksObject = KS.GetFromRequest();

            KalturaAssetFile response = new KalturaAssetFile();

            try
            {
                string userId = ksObject != null ? ksObject.UserId : "0";
                string udid = ksObject != null ? KSUtils.ExtractKSPayload(ksObject).UDID : string.Empty;

                bool isTokenizedUrl = !string.IsNullOrEmpty(tokenizedUrl);
                response.Url = ClientsManager.ConditionalAccessClient().GetPlayManifest(partnerId, userId, assetId, assetType, assetFileId, udid, contextType, isTokenizedUrl, isAltUrl);

                if (isTokenizedUrl)
                {
                    response.Url = Encoding.UTF8.GetString(Convert.FromBase64String(Utils.Utils.ReturnSlashesToBase64Str(tokenizedUrl)));
                }

                if (!string.IsNullOrEmpty(response.Url))
                {
                    // pass query string params
                    if (response.Url.ToLower().Contains("playmanifest"))
                    {
                        if (HttpContext.Current.Request.GetQueryString().Count > 0)
                        {
                            string dynamicQueryStringParamsConfiguration = ApplicationConfiguration.Current.PlayManifestDynamicQueryStringParamsNames.Value;
                            
                            // old fix for passing query string params - not using dynamic configuration
                            string[] dynamicQueryStringParamsNames;
                            if (string.IsNullOrEmpty(dynamicQueryStringParamsConfiguration))
                            {
                                dynamicQueryStringParamsNames = new string[] { "clientTag", "playSessionId" };
                            }
                            else
                            {
                                dynamicQueryStringParamsNames = dynamicQueryStringParamsConfiguration.Split(',');
                            }

                            foreach (var dynamicParam in dynamicQueryStringParamsNames)
                            {
                                if (!string.IsNullOrEmpty(HttpContext.Current.Request.GetQueryString()[dynamicParam]))
                                {
                                    response.Url = string.Format("{0}{1}{2}={3}", response.Url, response.Url.Contains("?") ? "&" : "?", dynamicParam, HttpContext.Current.Request.GetQueryString()[dynamicParam]);
                                }
                            }
                        }
                    }

                    string responseFormat = HttpContext.Current.Request.GetQueryString()["responseFormat"];

                    if (string.IsNullOrEmpty(responseFormat))
                    {
                        HttpContext.Current.Response.Redirect(response.Url);
                    }
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