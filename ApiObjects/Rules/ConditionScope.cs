using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Rules
{
    public interface ISegmentsConditionScope 
    {
        List<long> SegmentIds { get; set; }

        bool FilterBySegments { get; set; }
    }

    public interface IBusinessModuleConditionScope 
    {
        long BusinessModuleId { get; set; }

        eTransactionType? BusinessModuleType { get; set; }
    }

    public interface IDateConditionScope 
    {
        bool FilterByDate { get; set; }
    }

    public interface IHeaderConditionScope
    {
        Dictionary<string, string> Headers { get; set; }
    }

    public interface IIpRangeConditionScope
    {
        long Ip { get; set; }
    }

    public class ConditionScope : IBusinessModuleConditionScope, ISegmentsConditionScope, IDateConditionScope, IHeaderConditionScope, IIpRangeConditionScope
    {
        public long BusinessModuleId { get; set; }

        public eTransactionType? BusinessModuleType { get; set; }

        public bool FilterByDate { get; set; }

        public bool FilterBySegments { get; set; }
       
        public List<long> SegmentIds { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public long Ip { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (BusinessModuleId > 0)
            {
                sb.AppendFormat("BusinessModuleId: {0}; ", BusinessModuleId);
            }

            if (BusinessModuleType.HasValue)
            {
                sb.AppendFormat("BusinessModuleType: {0}; ", BusinessModuleType.Value);
            }

            if (FilterByDate)
            {
                sb.AppendFormat("FilterByDate: {0}; ", FilterByDate);
            }

            if (FilterBySegments)
            {
                sb.AppendFormat("FilterBySegments: {0}; ", FilterBySegments);
            }
            
            if (SegmentIds != null && SegmentIds.Count > 0)
            {
                sb.AppendFormat("SegmentIds: {0}; ", string.Join(",", SegmentIds));
            }

            if (Headers != null && Headers.Count > 0)
            {
                sb.AppendFormat("Headers: {0}; ", string.Join(",", Headers));
            }

            if (Ip > 0)
            {
                sb.AppendFormat("Ip: {0}; ", Ip);
            }
            
            return sb.ToString();
        }
    }
}