namespace WebAPI.Models.General
{
    public enum KalturaReportOrderBy
    {
        NONE
    }
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