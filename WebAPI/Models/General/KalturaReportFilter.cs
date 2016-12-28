using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.General
{
    public enum KalturaReportOrderBy
    {
        NONE
    }
    /// <summary>
    /// Report filter
    /// </summary>
    abstract public class KalturaReportFilter : KalturaFilter<KalturaReportOrderBy>
    {
        public override KalturaReportOrderBy GetDefaultOrderByValue()
        {
            return KalturaReportOrderBy.NONE;
        }    
    }
}