using ApiObjects.Response;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Utils;
using WebAPI.ModelsValidators;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.Controllers
{
    [Service("assetStruct")]
    public class AssetStructController : IKalturaController
    {
        /// <summary>
        /// Return a list of asset structs for the account with optional filter
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <returns></returns>
        [Action("list")]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ApiAuthorize]
        public static KalturaAssetStructListResponse List(KalturaBaseAssetStructFilter filter = null)
        {
            filter = filter ?? new KalturaAssetStructFilter();
            filter.Validate();

            return ClientsManager.CatalogClient().GetAssetStructs(KS.GetFromRequest().GroupId, filter);
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
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.MetaIdsDoesNotExist)]
        [Throws(eResponseStatus.AssetStructMissingBasicMetaIds)]
        [Throws(eResponseStatus.MetaIdsDuplication)]
        [Throws(eResponseStatus.AssetStructMetasConatinSystemNameDuplication)]
        static public KalturaAssetStruct Add(KalturaAssetStruct assetStruct)
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
        /// <param name="id">AssetStruct Identifier, id = 0 is identified as program AssetStruct</param>
        /// <param name="assetStruct">AssetStruct Object</param>
        /// <returns></returns>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.AssetStructNameAlreadyInUse)]
        [Throws(eResponseStatus.AssetStructSystemNameAlreadyInUse)]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.MetaIdsDoesNotExist)]
        [Throws(eResponseStatus.CanNotChangePredefinedAssetStructSystemName)]
        [Throws(eResponseStatus.AssetStructMissingBasicMetaIds)]
        [Throws(eResponseStatus.ParentIdShouldNotPointToItself)]
        [Throws(eResponseStatus.MetaIdsDuplication)]
        [Throws(eResponseStatus.AssetStructMetasConatinSystemNameDuplication)]
        [Throws(eResponseStatus.CanNotRemoveMetaIdsForLiveToVod)]
        [SchemeArgument("id", MinLong = 0)]
        static public KalturaAssetStruct Update(long id, KalturaAssetStruct assetStruct)
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
        /// <param name="id">AssetStruct Identifier, id = 0 is identified as program AssetStruct</param>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        [Throws(eResponseStatus.CanNotDeletePredefinedAssetStruct)]
        [Throws(eResponseStatus.CanNotDeleteParentAssetStruct)]
        [Throws(eResponseStatus.CannotDeleteAssetStruct)]
        [SchemeArgument("id", MinLong = 0)]
        static public bool Delete(long id)
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

        /// <summary>
        ///  Get AssetStruct by ID
        /// </summary>
        /// <param name="id">ID to get</param>
        /// <returns></returns>
        [Action("get")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetStructDoesNotExist)]
        static public KalturaAssetStruct Get(long id)
        {
            KalturaAssetStruct response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.CatalogClient().GetAssetStruct(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}