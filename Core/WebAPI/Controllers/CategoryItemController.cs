using ApiLogic.Catalog;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;

namespace WebAPI.Controllers
{
    [Service("categoryItem")]
    [AddAction(Summary = "categoryItem add",
        ObjectToAddDescription = "categoryItem details",
        ClientThrows = new eResponseStatus[]{
                    eResponseStatus.NameRequired,
                    eResponseStatus.CategoryNotExist,
                    eResponseStatus.ChannelDoesNotExist,
                    eResponseStatus.ChildCategoryNotExist,
                    eResponseStatus.ChildCategoryAlreadyBelongsToAnotherCategory,
                    eResponseStatus.CategoryTypeNotExist,
                    eResponseStatus.CategoryIsAlreadyAssociatedToVersion
        }
     )]

    [UpdateAction(Summary = "categoryItem update",
        IdDescription = "Category identifier",
        ObjectToUpdateDescription = "categoryItem details",
        ClientThrows = new eResponseStatus[] {
            eResponseStatus.CategoryNotExist,
            eResponseStatus.NameRequired,
            eResponseStatus.ChannelDoesNotExist,
            eResponseStatus.ChildCategoryNotExist,
            eResponseStatus.ParentIdShouldNotPointToItself,
            eResponseStatus.ChildCategoryCannotBeTheCategoryItself,
            eResponseStatus.ChildCategoryAlreadyBelongsToAnotherCategory,
            eResponseStatus.InvalidValue,
            eResponseStatus.CategoryVersionIsNotDraft,
            eResponseStatus.CategoryIsAlreadyAssociatedToVersion
        }
    )]

    [DeleteAction(Summary = "Remove category",
        IdDescription = "Category identifier",
        ClientThrows = new eResponseStatus[] { 
            eResponseStatus.CategoryNotExist,
            eResponseStatus.ImageDoesNotExist,
            eResponseStatus.CategoryVersionIsNotDraft,
            eResponseStatus.CategoryItemIsRoot }
    )]

    [ListAction(Summary = "Gets all categoryItem items", IsFilterOptional = true, IsPagerOptional = true)]
    public class CategoryItemController : KalturaCrudController<KalturaCategoryItem, KalturaCategoryItemListResponse, CategoryItem, long, KalturaCategoryItemFilter>
    {
    }
}