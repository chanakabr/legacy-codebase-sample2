using Core.Catalog;
using Phoenix.Generated.Api.Events.Crud.ProgramAsset;
using Image = Core.Catalog.Image;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface IProgramAssetCrudEventMapper
    {
        Image Map(Phoenix.Generated.Api.Events.Crud.ProgramAsset.Image image);
        LiveToVodAsset MapToAssetForAdd(ProgramAsset programAsset, LiveAsset liveAsset, int retentionPeriodInDays);

        LiveToVodAsset MapToAssetForUpdate(
            ProgramAsset programAsset,
            LiveAsset liveAsset,
            LiveToVodAsset currentAsset,
            int retentionPeriodInDays);
        ProgramAsset Map(EpgAsset epgAsset, LiveAsset liveAsset, long groupId, long updaterId, long operation);
    }
}