using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ModelsValidators
{
    public static class SeriesRecordingOptionValidator
    {
        public static void Validate(this KalturaSeriesRecordingOption model)
        {
            if (model.MinSeasonNumber.HasValue && !model.MinEpisodeNumber.HasValue)
                throw new System.Exception("Can't use minimal season without minimal episode");
            
            if (!model.MinSeasonNumber.HasValue && model.MinEpisodeNumber.HasValue)
                throw new System.Exception("Can't use minimal episode without minimal season");
        }
    }
}