using ApiLogic.Catalog;
using ApiObjects.Response;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;

namespace WebAPI.Controllers
{
    [Service("categoryItem")]
    [AddAction(Summary = "categoryItem add",
        ObjectToAddDescription = "categoryItem details",
        ClientThrows = new []{
                    eResponseStatus.NameRequired,
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
        ClientThrows = new[] {
            eResponseStatus.CategoryNotExist,
            eResponseStatus.NameRequired,
            eResponseStatus.ChannelDoesNotExist,
            eResponseStatus.ChildCategoryNotExist,
            eResponseStatus.ParentIdShouldNotPointToItself,
            eResponseStatus.ChildCategoryCannotBeTheCategoryItself,
            eResponseStatus.ChildCategoryAlreadyBelongsToAnotherCategory,
            eResponseStatus.CategoryVersionIsNotDraft,
            eResponseStatus.CategoryIsAlreadyAssociatedToVersion,
            eResponseStatus.StartDateShouldBeLessThanEndDate,
            eResponseStatus.AssetStructDoesNotExist,
            eResponseStatus.MetaDoesNotExist,
            eResponseStatus.InvalidMetaType,
            eResponseStatus.InvalidValueSentForMeta,
            eResponseStatus.AssetDoesNotExist,
            eResponseStatus.ActionIsNotAllowed,
            eResponseStatus.RelatedEntitiesExceedLimitation,
            eResponseStatus.DeviceRuleDoesNotExistForGroup,
            eResponseStatus.GeoBlockRuleDoesNotExistForGroup,
            eResponseStatus.AssetExternalIdMustBeUnique
        }
    )]

    [DeleteAction(Summary = "Remove category",
        IdDescription = "Category identifier",
        ClientThrows = new [] {
            eResponseStatus.CategoryNotExist,
            eResponseStatus.ImageDoesNotExist,
            eResponseStatus.CategoryVersionIsNotDraft,
            eResponseStatus.CategoryItemIsRoot }
    )]

    [ListAction(
        Summary = "Gets all categoryItem items",
        IsFilterOptional = true,
        IsPagerOptional = true,
        ClientThrows = new [] { eResponseStatus.CategoryTypeNotExist, eResponseStatus.InvalidValue, eResponseStatus.CategoryNotExist })]
    public class CategoryItemController : KalturaCrudController<KalturaCategoryItem, KalturaCategoryItemListResponse, CategoryItem, long, KalturaCategoryItemFilter>
    {
    }
}