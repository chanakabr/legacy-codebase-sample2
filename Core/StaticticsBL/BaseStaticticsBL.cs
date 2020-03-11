using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Statistics;

namespace StatisticsBL
{
    public abstract class BaseStaticticsBL
    {
        public int m_nGroupID { get; protected set; }


        public abstract BuzzWeightedAverScore GetBuzzAverScore(string sKey);

        public abstract Dictionary<string, BuzzWeightedAverScore> GetBuzzAverScore(List<int> lKey);
    }
}
