namespace WebAPI.Models.General
{
    /// <summary>
    /// DynamicListFilter
    /// </summary>
    public partial class KalturaDynamicListFilter : KalturaFilter<KalturaDynamicListOrderBy>
    {
        public override KalturaDynamicListOrderBy GetDefaultOrderByValue()
        {
            return KalturaDynamicListOrderBy.NONE;
        }
    }
}