using ApiObjects.SearchObjects;
using ApiObjects.SearchObjects.GroupRepresentatives;
using Core.Catalog.Request;

namespace ApiLogic.Catalog.CatalogManagement.Services.GroupRepresentatives
{
    public interface IGroupRepresentativesExtendedRequestMapper
    {
        ExtendedSearchRequest BuildRequest(GroupRepresentativesRequest request, CatalogClientData clientData, string extraReturnField);
        ExtendedSearchRequest BuildEntitledRequest(GroupRepresentativesRequest request, CatalogClientData clientData, string extraReturnField);
        ExtendedSearchRequest BuildRequest(GroupRepresentativesRequest request, CatalogClientData clientData, BooleanPhraseNode filterTree);
    }
}