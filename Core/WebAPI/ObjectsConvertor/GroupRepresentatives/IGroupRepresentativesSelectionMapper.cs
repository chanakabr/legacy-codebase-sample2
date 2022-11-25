using ApiObjects.SearchObjects;
using ApiObjects.SearchObjects.GroupRepresentatives;
using WebAPI.Models.Catalog.GroupRepresentatives;

namespace WebAPI.ObjectsConvertor.GroupRepresentatives
{
    public interface IGroupRepresentativesSelectionMapper
    {
        UnmatchedItemsPolicy MapToUnmatchedItemsPolicy(KalturaUnmatchedItemsPolicy? policy);
        RepresentativeSelectionPolicy MapToRepresentativeSelectionPolicy(KalturaRepresentativeSelectionPolicy policy);
    }
}