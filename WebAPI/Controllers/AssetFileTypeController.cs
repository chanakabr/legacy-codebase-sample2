using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Models.API;
using WebAPI.Utils;
using WebAPI.Managers.Models;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using ApiObjects.Response;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/assetFileType/action")]
    public class AssetFileTypeController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
       
        /// <summary>
        /// Returns a list of asset-file types
        /// </summary>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetFileTypeListResponse List()
        {
            KalturaAssetFileTypeListResponse response = new KalturaAssetFileTypeListResponse();

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            try
            {
                response = ClientsManager.CatalogClient().GetAssetFileTypes(groupId);
            }
            catch (ClientException ex)
            {
                throw new ApiException(ex);
            }

            return response;
        }

        /// <summary>
        /// Add new asset-file type
        /// </summary>
        /// <param name="assetFileType">Asset-file type</param>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetFileTypeNameAlreadyInUse)]
        public KalturaAssetFileType Add(KalturaAssetFileType assetFileType)
        {
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            assetFileType.validateForInsert();

            try
            {
                return ClientsManager.CatalogClient().AddAssetFileType(groupId, assetFileType, userId);
            }
            catch (ClientException ex)
            {
                throw new ApiException(ex);
            }
        }

        /// <summary>
        /// Update existing asset-file type
        /// </summary>
        /// <param name="id">Asset-file type identifier</param>
        /// <param name="assetFileType">Asset-file type</param>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetFileTypeNameAlreadyInUse)]
        [Throws(eResponseStatus.AssetFileTypeDoesNotExist)]
        public KalturaAssetFileType Update(int id, KalturaAssetFileType assetFileType)
        {
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                return ClientsManager.CatalogClient().UpdateAssetFileType(groupId, id, assetFileType, userId);
            }
            catch (ClientException ex)
            {
                throw new ApiException(ex);
            }
        }

        /// <summary>
        /// Delete asset-file type by id
        /// </summary>
        /// <param name="id">Asset-file type identifier</param>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetFileTypeDoesNotExist)]
        public bool Delete(int id)
        {
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                return ClientsManager.CatalogClient().DeleteAssetFileType(groupId, id, userId);
            }
            catch (ClientException ex)
            {
                throw new ApiException(ex);
            }
        }
    }
}