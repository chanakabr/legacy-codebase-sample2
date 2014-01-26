using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class DiscountModule
    {
        public double percent { get; set; }

        public RelationTypes theRelationType { get; set; }

        public System.DateTime startDate { get; set; }

        public System.DateTime endDate { get; set; }

        public WhenAlgo whenAlgo { get; set; }
    }
}
