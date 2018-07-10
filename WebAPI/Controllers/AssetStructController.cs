using ApiObjects.Response;
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
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("assetStruct")]
    public class AssetStructController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Return a list of asset structs for the account with optional filter
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        public KalturaAssetStructListResponse List(KalturaAssetStructFilter filter = null)
        {
            if (filter == null)
            {
                filter = new KalturaAssetStructFilter();
            }

            KalturaAssetStructListResponse response = new KalturaAssetStructListResponse();
            int groupId = KS.GetFromRequest().GroupId;
            try
            {
                filter.Validate();
                if (filter.MetaIdEqual.HasValue && filter.MetaIdEqual.Value > 0)
                {                    
                    response = ClientsManager.CatalogClient().GetAssetStructs(groupId, new List<long>(), filter.OrderBy, filter.IsProtectedEqual, filter.MetaIdEqual.Value);
                }
                else
                {                   
                    response = ClientsManager.CatalogClient().GetAssetStructs(groupId, filter.GetIdIn(), filter.OrderBy, filter.IsProtectedEqual);
                }

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Add a new assetStruct
        /// </summary>
        /// <param name="assetStruct">AssetStruct Object</param>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetStructNameAlreadyInUse)]
        [Throws(eResponseStatus.AssetStructSystemNameAlreadyInUse)]
        [Throws(eResponseStatus.MetaIdsDoesNotExist)]
        [Throws(eResponseStatus.AssetStructMissingBasicMetaIds)]
        public KalturaAssetStruct Add(KalturaAssetStruct assetStruct)
        {
            KalturaAssetStruct response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();
            if (assetStruct.Name == null || assetStruct.Name.Values == null || assetStruct.Name.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            assetStruct.Name.Validate("multilingualName");
            if (string.IsNullOrEmpty(assetStruct.SystemName) || string.IsNullOrEmpty(assetStruct.SystemName.Trim()))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");
            }

            assetStruct.Validate();
            try
            {                
                response = ClientsManager.CatalogClient().AddAssetStruct(groupId, assetStruct, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update an existing assetStruct
        /// </summary>
        /// <param name="id">AssetStruct Identifier</param>
        /// <param name="assetStruct">AssetStruct Object</param>
        /// <returns></returns>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.AssetStructNameAlreadyInUse)]
        [Throws(eResponseStatus.AssetStructSystemNameAlreadyInUse)]
        [Throws(eResponseStatus.MetaIdsDoesNotExist)]
        [Throws(eResponseStatus.CanNotChangePredefinedAssetStructSystemName)]
        [Throws(eResponseStatus.AssetStructMissingBasicMetaIds)]
        [Throws(eResponseStatus.ParentIdShouldNotPointToItself)]
        [SchemeArgument("id", MinLong = 1)]
        public KalturaAssetStruct Update(long id, KalturaAssetStruct assetStruct)
        {
            KalturaAssetStruct response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();
            if (assetStruct.Name != null)
            {
                if ((assetStruct.Name.Values == null || assetStruct.Name.Values.Count == 0))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }
                else
                {
                    assetStruct.Name.Validate("multilingualName");
                }
            }

            if (assetStruct.SystemName != null &&  assetStruct.SystemName.Trim() == string.Empty)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");
            }
            
            assetStruct.Validate();

            try
            {                
                response = ClientsManager.CatalogClient().UpdateAssetStruct(groupId, id, assetStruct, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete an existing assetStruct
        /// </summary>
        /// <param name="id">AssetStruct Identifier</param>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.CanNotDeletePredefinedAssetStruct)]
        [Throws(eResponseStatus.CanNotDeleteParentAssetStruct)]
        [SchemeArgument("id", MinLong = 1)]
        public bool Delete(long id)
        {
            bool result = false;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                result = ClientsManager.CatalogClient().DeleteAssetStruct(groupId, id, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

    }
}