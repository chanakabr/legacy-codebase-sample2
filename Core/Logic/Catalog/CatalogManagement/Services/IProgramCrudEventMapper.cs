using Core.Catalog;
using Phoenix.Generated.Api.Events.Crud.ProgramAsset;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface IProgramCrudEventMapper
    {
        ProgramAsset Map(EpgAsset epgAsset, LiveAsset liveAsset, long groupId, long updaterId, long operation);
    }
}