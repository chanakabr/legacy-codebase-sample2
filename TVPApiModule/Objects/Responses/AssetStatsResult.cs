using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Responses
{
    public class AssetStatsResult
    {
        public BuzzWeightedAverScore buuz_average_score { get; set; }

        public double rate { get; set; }

        public int asset_id { get; set; }

        public int likes { get; set; }

        public int views { get; set; }

        public int votes { get; set; }
    }
}
