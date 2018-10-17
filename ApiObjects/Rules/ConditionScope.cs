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

    public class ConditionScope : IBusinessModuleConditionScope, ISegmentsConditionScope, IDateConditionScope
    {
        public long BusinessModuleId { get; set; }

        public eTransactionType? BusinessModuleType { get; set; }

        public bool FilterByDate { get; set; }

        public bool FilterBySegments { get; set; }
       
        public List<long> SegmentIds { get; set; }

    }
}


