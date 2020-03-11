using WebAPI.Models.General;


namespace WebAPI.Models.API
{
    public enum KalturaSearchHistoryOrderBy
    {
        NONE
    }

    public partial class KalturaSearchHistoryFilter : KalturaFilter<KalturaSearchHistoryOrderBy>
    {
        public override KalturaSearchHistoryOrderBy GetDefaultOrderByValue()
        {
            return KalturaSearchHistoryOrderBy.NONE;
        }
    }
}