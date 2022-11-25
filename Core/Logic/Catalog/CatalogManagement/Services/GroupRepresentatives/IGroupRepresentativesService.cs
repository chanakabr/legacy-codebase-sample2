using ApiLogic.Catalog.Response;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.SearchObjects.GroupRepresentatives;

namespace ApiLogic.Catalog.CatalogManagement.Services.GroupRepresentatives
{
    public interface IGroupRepresentativesService
    {
        GenericResponse<GroupRepresentativesResult> GetGroupRepresentativeList(
            GroupRepresentativesRequest request,
            CatalogClientData clientData);
    }
}