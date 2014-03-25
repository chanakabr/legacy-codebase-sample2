using BuzzFeeder.BuzzCalculator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase.Extensions;

namespace BuzzFeeder.Implementation.Channels
{
    public class TvinciChannelCommentsBuzzImpl : BaseChannelBuzzImpl
    {
        private readonly string m_sGroupKey;

        public TvinciChannelCommentsBuzzImpl(int nGroupID, DateTime dtPeriod, TimeSpan dtInterval, List<string> lActions, List<string> lMediaTypes)
            : base(nGroupID, dtPeriod, dtInterval, lActions, lMediaTypes)
        {
            m_sGroupKey = string.Concat(m_nGroupID, "_channel_comments_stats");
        }

        protected override string GetGroupKey()
        {
            return m_sGroupKey;
        }
    }
}
