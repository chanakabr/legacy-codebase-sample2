using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzFeeder.BuzzCalculator
{
    public class BuzzCalculator
    {

        public BuzzCalculator(List<int> lFormulaWeights)
        {
            m_lFormualWeights = lFormulaWeights;
        }

        protected List<int> m_lFormualWeights;

        public Dictionary<string, ItemsStats> m_dCurBuzzCount { get; set; }
        public Dictionary<string, ItemsStats> m_dPrevBuzzCount { get; set; }

        public double m_nCurGroupSampleCount { get; set; }
        public double m_nCurGroupSampleCumulativeCount { get; set; }
        public double m_nPrevGroupSampleCount { get; set; }
        public double m_nPrevGroupSampleCumulativeCount { get; set; }
        protected double m_nCurSampleGroupAverage;
        protected double m_nPrevSampleGroupAverage;

        public Dictionary<string, ItemsStats> m_dItemStats { get; protected set; }

        public void CalcBuzz()
        {
            InitItemStats();

            CalculateItemPeriodicalGrowth();
            CalculateDeltaFromAverage(m_nCurSampleGroupAverage);
            CalculateRelativePeriocialGrowth();
            CalculateCumulativePeriocialGrowth();
            CalculateActivityMeasurement();
        }

        private void InitItemStats()
        {
            m_dItemStats = new Dictionary<string, ItemsStats>();

            ItemsStats tempStats;
            double nSum = 0.0;
            double nCumulativeSum = 0.0;

            foreach (string sItemID in m_dCurBuzzCount.Keys)
            {
                tempStats = m_dCurBuzzCount[sItemID];
                m_dItemStats[sItemID] = new ItemsStats() { nSampleCount = tempStats.nSampleCount, nSampleCumulativeCount = tempStats.nSampleCumulativeCount, sMediaID = tempStats.sMediaID };
                nSum += tempStats.nSampleCount;
                nCumulativeSum += tempStats.nSampleCumulativeCount;
            }

            m_nCurGroupSampleCount = nSum;
            m_nCurGroupSampleCumulativeCount = nCumulativeSum;
            m_nCurSampleGroupAverage = (m_dCurBuzzCount.Count != 0) ? m_nCurGroupSampleCount / m_dCurBuzzCount.Count : 1;

            nSum = 0.0;
            nCumulativeSum = 0.0;
            foreach (string sItemID in m_dPrevBuzzCount.Keys)
            {
                tempStats = m_dPrevBuzzCount[sItemID];
                nSum += tempStats.nSampleCount;
                nCumulativeSum += tempStats.nSampleCumulativeCount;
            }

            m_nPrevGroupSampleCount = nSum;
            m_nPrevGroupSampleCumulativeCount = nCumulativeSum;
            m_nPrevSampleGroupAverage = (m_dPrevBuzzCount.Count != 0) ? m_nPrevGroupSampleCount / m_dPrevBuzzCount.Count : 1;
        }

        protected void CalculateItemPeriodicalGrowth()
        {
            foreach (string itemID in m_dCurBuzzCount.Keys)
            {
                if (m_dPrevBuzzCount.ContainsKey(itemID) && m_dPrevBuzzCount[itemID].nSampleCount > 0)
                {
                    m_dItemStats[itemID].nPeriodicalGrowth = (m_dCurBuzzCount[itemID].nSampleCount / m_dPrevBuzzCount[itemID].nSampleCount) - 1;
                }
                else
                {
                    m_dItemStats[itemID].nPeriodicalGrowth = m_dCurBuzzCount[itemID].nSampleCount;
                }
            }
        }

        protected void CalculateDeltaFromAverage(double nGroupAvg)
        {
            //if nGroupAvg == 0, this means that no actions were made and thus all sample counts are 0.
            if (nGroupAvg == 0)
            {
                return;
            }

            foreach (string sItemID in m_dCurBuzzCount.Keys)
            {
                m_dItemStats[sItemID].nDeltaFromGroupAverage = (m_dCurBuzzCount[sItemID].nSampleCount / nGroupAvg) - 1;
            }
        }

        protected void CalculateRelativePeriocialGrowth()
        {
            double nCurSampleCumCountExclude, nPrevSampleCumCountExclude;

            foreach (string sItemID in m_dCurBuzzCount.Keys)
            {
                if (m_dPrevBuzzCount.ContainsKey(sItemID) && m_dPrevBuzzCount[sItemID].nSampleCount != 0)
                {
                    nCurSampleCumCountExclude = m_nCurGroupSampleCount - m_dCurBuzzCount[sItemID].nSampleCount;
                    nPrevSampleCumCountExclude = m_nPrevGroupSampleCount - m_dPrevBuzzCount[sItemID].nSampleCount;
                    if (nPrevSampleCumCountExclude != 0)
                    {
                        m_dItemStats[sItemID].nRelativePeriodicalGrowth = (m_dCurBuzzCount[sItemID].nSampleCount / m_dPrevBuzzCount[sItemID].nSampleCount) - (nCurSampleCumCountExclude / nPrevSampleCumCountExclude);
                    }
                        
                    else
                    {
                        m_dItemStats[sItemID].nRelativePeriodicalGrowth = m_dCurBuzzCount[sItemID].nSampleCount;
                    }
                }
                else
                {
                    m_dItemStats[sItemID].nRelativePeriodicalGrowth = m_dCurBuzzCount[sItemID].nSampleCount;
                }
            }
        }

        protected void CalculateCumulativePeriocialGrowth()
        {
            double nCurSampleCumCountExclude, nPrevSampleCumCountExclude;

            foreach (string sItemID in m_dCurBuzzCount.Keys)
            {
                if (m_dPrevBuzzCount.ContainsKey(sItemID) && m_dPrevBuzzCount[sItemID].nSampleCumulativeCount != 0)
                {
                    nCurSampleCumCountExclude = m_nCurGroupSampleCumulativeCount - m_dCurBuzzCount[sItemID].nSampleCumulativeCount;
                    nPrevSampleCumCountExclude = m_nPrevGroupSampleCumulativeCount - m_dPrevBuzzCount[sItemID].nSampleCumulativeCount;
                    if (nPrevSampleCumCountExclude != 0)
                    {
                        m_dItemStats[sItemID].nRelativeCumulativeGrowth = (m_dCurBuzzCount[sItemID].nSampleCumulativeCount / m_dPrevBuzzCount[sItemID].nSampleCumulativeCount) - (nCurSampleCumCountExclude / nPrevSampleCumCountExclude);
                    }
                    else
                    {
                        m_dItemStats[sItemID].nRelativeCumulativeGrowth = m_dCurBuzzCount[sItemID].nSampleCumulativeCount;
                    }
                }
                else
                {
                    m_dItemStats[sItemID].nRelativeCumulativeGrowth = m_dCurBuzzCount[sItemID].nSampleCumulativeCount;
                }
            }
        }

        protected void CalculateActivityMeasurement()
        {
            foreach (var item in m_dItemStats.Values)
            {
                item.nActivityMeasurement = (m_lFormualWeights[0] * item.nPeriodicalGrowth) + (m_lFormualWeights[1] * item.nDeltaFromGroupAverage) + (m_lFormualWeights[2] * item.nRelativePeriodicalGrowth) + (m_lFormualWeights[3] * item.nRelativeCumulativeGrowth);
            }
        }

   
    }
}
