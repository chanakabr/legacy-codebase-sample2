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
    //[Service("categoryTree")]
    //[GetAction(Summary = "Get categoryTree by categoryTree identifier", IsFilterOptional = true)]
    //public class CategoryTreeController : KalturaCrudController<KalturaCategoryTree, KalturaCategoryTreeListResponse, CategoryTree, long, KalturaCategoryTreeFilter, CategoryTreeFilter>
    //{
    //    /// <summary>
    //    /// Sign up a new user.      
    //    /// </summary>        
    //    /// <param name="id">Category identifier</param>        
    //    [Action("duplicate")]
    //    [ValidationException(SchemeValidationType.ACTION_NAME)]
    //    static public KalturaCategoryTree Duplicate(long id)
    //    {
    //        KalturaCategoryTree response = null;

    //        var groupId = KS.GetFromRequest().GroupId;

    //        try
    //        {
    //            response = ClientsManager.CatalogClient().Duplicate(groupId, id);
    //        }
    //        catch (ClientException ex)
    //        {
    //            ErrorUtils.HandleClientException(ex);
    //        }

    //        return response;
    //    }
    //}
}