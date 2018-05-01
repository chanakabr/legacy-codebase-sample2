using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/assetStructMeta/action")]
    public class AssetStructMetaController : ApiController
    {
        /// <summary>
        /// Return a list of asset struct metas for the account with optional filter
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetStructMetaListResponse List(KalturaAssetStructMetaFilter filter)
        {
            KalturaAssetStructMetaListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "filter");
            }

            filter.Validate();

            try
            {    
                response = ClientsManager.CatalogClient().GetAssetStructMetaList(groupId, filter.AssetStructIdEqual, filter.MetaIdEqual);                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update Asset struct meta
        /// </summary>
        /// <param name="assetStructId">AssetStruct Identifier</param>
        /// <param name="metaId">Meta Identifier</param>
        /// <param name="assetStructMeta">AssetStructMeta Object</param>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.MetaDoesNotExist)]
        [SchemeArgument("assetStructId", MinLong = 1)]
        [SchemeArgument("metaId", MinLong = 1)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public KalturaAssetStructMeta Update(long assetStructId, long metaId, KalturaAssetStructMeta assetStructMeta)
        {
            KalturaAssetStructMeta response = null;
            
            if (assetStructMeta.IngestReferencePath == null &&
                assetStructMeta.DefaultIngestValue == null &&
                !assetStructMeta.ProtectFromIngest.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "IngestReferencePath", "DefaultIngestValue", "ProtectFromIngest");
            }

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                response = ClientsManager.CatalogClient().UpdateAssetStructMeta(assetStructId, metaId, assetStructMeta, groupId, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}