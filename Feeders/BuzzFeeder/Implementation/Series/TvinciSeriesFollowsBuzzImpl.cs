using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzFeeder.Implementation.Series
{
    public class TvinciSeriesFollowsBuzzImpl : BaseSeriesBuzzImpl
    {
        private readonly string m_sGroupKey;

        public TvinciSeriesFollowsBuzzImpl(int nGroupID, DateTime dtPeriod, TimeSpan tsInterval, List<string> lActions, List<string> lMediaTypes)
            : base(nGroupID, dtPeriod, tsInterval, lActions, lMediaTypes)
        {
            m_sGroupKey = string.Concat(m_nGroupID, "_series_follows_stats");
        }

        protected override string GetGroupKey()
        {
            return m_sGroupKey;
        }
    }
}
