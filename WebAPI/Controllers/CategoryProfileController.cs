using ApiLogic.Catalog;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;

namespace WebAPI.Controllers
{
    [Service("categoryProfile")]
    [AddAction(Summary = "categoryProfile add",
               ObjectToAddDescription = "categoryProfile details"//,
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

    [ListAction(Summary = "Gets all categoryProfile items for a household", IsFilterOptional = true)]
    public class CategoryProfileController : KalturaCrudController<KalturaCategoryProfile, KalturaCategoryProfileListResponse, CategoryProfile, long, KalturaCategoryProfileFilter, CategoryProfileFilter>
    {
    }    
}