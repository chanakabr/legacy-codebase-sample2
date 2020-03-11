using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzFeeder.Implementation.Series
{
    public class TvinciSeriesViewsBuzzImpl : BaseSeriesBuzzImpl
    {
        private readonly string m_sGroupKey;

        public TvinciSeriesViewsBuzzImpl(int nGroupID, string sSeriesTagType, string[] lSeriesMediaTypeId, DateTime dtPeriod, TimeSpan tsInterval, int Weight, List<string> lActions, List<string> lAssetTypes, List<int> lFormulaWeights)
            : base(nGroupID, sSeriesTagType, lSeriesMediaTypeId, dtPeriod, tsInterval, Weight, lActions, lAssetTypes, lFormulaWeights)
        {
            m_sGroupKey = string.Concat(m_nGroupID, "_series_views_stats");
        }

        protected override string GetGroupKey()
        {
            return m_sGroupKey;
        }
    }
}
