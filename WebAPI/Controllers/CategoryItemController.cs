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
    [Service("categoryItem")]
    [AddAction(Summary = "categoryItem add",
               ObjectToAddDescription = "categoryItem details"//,
                                                                 //ClientThrows = new eResponseStatus[]{
                                                                 //    eResponseStatus.HouseholdRequired,
                                                                 //    eResponseStatus.ObjectNotExist
                                                                 //}
               )]


    [UpdateAction(Summary = "categoryItem update",
                  IdDescription = "Category identifier",
                  ObjectToUpdateDescription = "categoryItem details"//,
                                                                       //ClientThrows = new eResponseStatus[]{
                                                                       //    eResponseStatus.HouseholdRequired,
                                                                       //    eResponseStatus.ObjectNotExist
                                                                       //}
               )]

    [DeleteAction(Summary = "Remove category",
                  IdDescription = "Category identifier"//,
                                                       //ClientThrows = new eResponseStatus[] { 
                                                       //    eResponseStatus.HouseholdRequired,
                                                       //    eResponseStatus.ObjectNotExist,
                                                       //}
                  )]

    [ListAction(Summary = "Gets all categoryItem items", IsFilterOptional = true)]
    public class CategoryItemController : KalturaCrudController<KalturaCategoryItem, KalturaCategoryItemListResponse, CategoryItem, long, KalturaCategoryItemFilter, CategoryItemFilter>
    {       
    }
}