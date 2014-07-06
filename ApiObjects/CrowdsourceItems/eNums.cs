namespace ApiObjects.CrowdsourceItems
{
    public enum eItemType
    {
        LinearViews,
        Recommendation,
        VOD
    }

    public enum eCrowdsourceType
    {
        LiveViews,
        SlidingWindow,
        Orca
    }

    public enum eGalleryType
    {
        HomeVODPersonal,
        HomeVODPromotions,
        HomeLivePersonal,
        HomeLivePromotions,
        CatalogVODPromotions,
        CategoryCatalogVODPromotions,
        MovieRelated,
        SeriesRelated,
        EpisodeRelated,
        PersonalRecommendationsVOD,
        PersonalRecommendationsLive,
        EndOfMovie,
        EndOfEpisode,
        VODAction,
        VODDrama,
        VODComedy,
        VODThriller,
        VODDoco,
        VODKids
    }
}
