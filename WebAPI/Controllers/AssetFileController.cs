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
    public class AssetFileController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// get KalturaAssetFileContext
        /// </summary>
        /// <param name="id">Asset file identifier</param>
        ///<param name=" contextType">Kaltura Context Type (none = 0, recording = 1)</param>
        /// <remarks></remarks>
        [Route("_service/assetFile/action/getContext"), HttpPost]
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
        /// <remarks></remarks>
        // assetId/{assetId}/assetType/{assetType}/assetFileId/{assetFileId}/ks/{ks}/seekFrom/{seekFrom}
        [Route("p/{partnerId}/playManifest/{*pathData}"), HttpPost, HttpGet]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [SchemeArgument("partnerId", MinInteger = 1)]
        [SchemeArgument("assetFileId", MinInteger = 1)]
        [FailureHttpCode(System.Net.HttpStatusCode.NotFound)]
        public async Task<object> GetPlayManifestWrapper(int partnerId = 0, string ks = null, string assetId = null, KalturaAssetType? assetType = null, long assetFileId = 0, KalturaPlaybackContextType? contextType = null)
        {
            MethodInfo methodInfo = null;
            ApiController classInstance = null;
            object response = null;

            ServiceController.CreateMethodInvoker("AssetFile", "GetPlayManifest", out methodInfo, out classInstance);

            try
            {
                List<object> methodParams = (List<object>)HttpContext.Current.Items[WebAPI.Filters.RequestParser.REQUEST_METHOD_PARAMETERS];
                response = methodInfo.Invoke(classInstance, methodParams.ToArray());
            }
            catch (ApiException ex)
            {
                ApiException apiEx = new ApiException(ex, System.Net.HttpStatusCode.InternalServerError);
                throw apiEx;
            }
            catch (TargetParameterCountException ex)
            {
                ApiException apiEx = new ApiException(new BadRequestException(BadRequestException.INVALID_ACTION_PARAMETERS), System.Net.HttpStatusCode.InternalServerError);
                throw apiEx;
            }
            catch (Exception ex)
            {
                log.Error("Failed to perform action", ex);

                if (ex.InnerException is ApiException)
                {
                    ApiException apiEx = new ApiException((ApiException)ex.InnerException, System.Net.HttpStatusCode.NotFound);
                    throw apiEx;
                }

                ApiException generalErrorEx = new ApiException(ex, System.Net.HttpStatusCode.InternalServerError);
                throw generalErrorEx;
            }

            return response;
        }

        public bool GetPlayManifest(int partnerId = 0, string ks = null, string assetId = null, KalturaAssetType? assetType = null, long assetFileId = 0, KalturaPlaybackContextType? contextType = null)
        {
            

            if (string.IsNullOrEmpty(assetId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "assetId");
            }

            if (!assetType.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "assetType");
            }

            if (!contextType.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "contextType");
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

                response = ClientsManager.ConditionalAccessClient().GetPlayManifest(partnerId, userId, assetId, assetType.Value, assetFileId, udid, contextType.Value);

                if (response != null)
                {
                    if (!string.IsNullOrEmpty(response))
                    {
                        Redirect(response);
                    }
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
                return false;
            }

            return true;
        }
    }
}