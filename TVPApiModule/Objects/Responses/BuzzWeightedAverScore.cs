using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Responses
{
    public class BuzzWeightedAverScore
    {
        public double normalized_weighted_average_score { get; set; }

        public DateTime update_date { get; set; }

        public double weighted_average_score { get; set; }
    }
}
