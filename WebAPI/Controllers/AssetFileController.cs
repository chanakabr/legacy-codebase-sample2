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
        /// assetId/{assetId}/assetType/{assetType}/assetFileId/{assetFileId}/ks/{ks}/seekFrom/{seekFrom}
        [Route("p/{partnerId}/playManifest"), HttpPost, HttpGet]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool GetPlayManifest(int partnerId, string assetId, KalturaAssetType assetType, long assetFileId, string ks, long seekFrom, KalturaContextType contextType)
        {
            if (partnerId == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "partnerId");
            }

            if (string.IsNullOrEmpty(assetId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "assetId");
            }

            if (assetType == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "assetType");
            }

            if (assetFileId == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "assetFileId");
            }

            if (assetType == KalturaAssetType.epg && seekFrom == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "seekFrom");
            }

            KS ksObject = KS.ParseKS(ks);

            if (!ksObject.IsValid)
            {
                throw new UnauthorizedException(UnauthorizedException.KS_EXPIRED);
            }

            if (partnerId != ksObject.GroupId)
            {
                throw new UnauthorizedException(UnauthorizedException.PARTNER_INVALID);
            }

            string response = null;

            try
            {
                string userId = ksObject.UserId;
                string udid = KSUtils.ExtractKSPayload(ksObject).UDID;

                response = ClientsManager.ConditionalAccessClient().GetAssetLicensedLink(partnerId, userId, assetId, assetType, assetFileId, udid, contextType, seekFrom);

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