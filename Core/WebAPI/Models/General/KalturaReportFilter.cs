namespace WebAPI.Models.General
{
    /// <summary>
    /// Report filter
    /// </summary>
    abstract public partial class KalturaReportFilter : KalturaFilter<KalturaReportOrderBy>
    {
        public override KalturaReportOrderBy GetDefaultOrderByValue()
        {
            return KalturaReportOrderBy.NONE;
        }    
    }
}