using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
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
        /// <remarks></remarks>
         [Route("getContext"), HttpPost]
         [ApiAuthorize]
         [ValidationException(SchemeValidationType.ACTION_NAME)]
         public KalturaAssetFileContext GetContext(string id)
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

                 response = ClientsManager.ConditionalAccessClient().GetAssetFileContext(groupId, userID, id, udid, language);

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
    }
}