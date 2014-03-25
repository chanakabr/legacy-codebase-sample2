using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzFeeder.Implementation.Series
{
    public class TvinciSeriesViewsBuzzImpl : BaseSeriesBuzzImpl
    {
        private readonly string m_sGroupKey;

        public TvinciSeriesViewsBuzzImpl(int nGroupID, DateTime dtPeriod, TimeSpan tsInterval, List<string> lActions, List<string> lAssetTypes)
            : base(nGroupID, dtPeriod, tsInterval, lActions, lAssetTypes)
        {
            m_sGroupKey = string.Concat(m_nGroupID, "_series_views_stats");
        }

        protected override string GetGroupKey()
        {
            return m_sGroupKey;
        }
    }
}
