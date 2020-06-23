using ApiObjects.Response;
using WebAPI.ClientManagers.Client;
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
    }
}