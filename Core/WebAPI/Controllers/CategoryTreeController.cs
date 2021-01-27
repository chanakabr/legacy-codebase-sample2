using ApiLogic.Catalog;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("categoryTree")]
    public class CategoryTreeController : IKalturaController
    {
        /// <summary>
        /// Duplicate category Item
        /// </summary>        
        /// <param name="categoryItemId">Category item identifier</param>  
        /// <param name="name">Root category name</param>  
        [Action("duplicate")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.CategoryNotExist)]
        static public KalturaCategoryTree Duplicate(long categoryItemId, string name)
        {
            KalturaCategoryTree response = null;

            var groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {

                response = ClientsManager.CatalogClient().Duplicate(groupId, long.Parse(userId), categoryItemId, name);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Retrive category tree.      
        /// </summary>        
        /// <param name="categoryItemId">Category item identifier</param>
        /// <param name="filter">filter categories dates</param>
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.CategoryNotExist)]
        static public KalturaCategoryTree Get(long categoryItemId, bool filter = false)
        {
            KalturaCategoryTree response = null;

            var groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, userId, true);

                response = ClientsManager.CatalogClient().GetCategoryTree(groupId, categoryItemId, filter, isAllowedToViewInactiveAssets);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Retrieve default category tree of deviceFamilyId by KS or specific one if versionId is set. 
        /// </summary>        
        /// <param name="versionId">Category version id of tree</param>
        [Action("getByVersion")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.CategoryVersionDoesNotExist)]
        static public KalturaCategoryTree GetByVersion(long? versionId = null)
        {
            KalturaCategoryTree response = null;
            var contextData = KS.GetContextData();

            try
            {
                Func<GenericResponse<CategoryTree>> getByVersionFunc = () =>
                    CategoryItemHandler.Instance.GetTreeByVersion(contextData, versionId);

                response = ClientUtils.GetResponseFromWS<KalturaCategoryTree, CategoryTree>(getByVersionFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}