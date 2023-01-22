using WebAPI.Models.Catalog;

namespace WebAPI.Validation
{
    public interface INextEpisodeValidator
    {
        void Validate(int groupId, KalturaSeriesIdArguments seriesIdArguments);
    }
}