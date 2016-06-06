using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearchHandler.IndexBuilders
{
    public interface IIndexBuilder
    {
        bool SwitchIndexAlias { get; set; }
        bool DeleteOldIndices { get; set; }
        DateTime? StartDate { get; set; }
        DateTime? EndDate { get; set; }

        bool BuildIndex();
    }
}
