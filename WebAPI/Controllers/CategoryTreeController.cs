using ApiLogic.Catalog;
using Core.Catalog.Handlers;
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
        /// Sign up a new user.      
        /// </summary>        
        /// <param name="categoryItemId">Category item identifier</param>        
        [Action("duplicate")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public KalturaCategoryTree Duplicate(long categoryItemId)
        {
            KalturaCategoryTree response = null;

            var groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.CatalogClient().Duplicate(groupId, categoryItemId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}