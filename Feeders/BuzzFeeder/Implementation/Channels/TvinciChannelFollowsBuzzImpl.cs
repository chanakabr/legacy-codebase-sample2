using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzFeeder.Implementation.Channels
{
    public class TvinciChannelFollowsBuzzImpl : BaseChannelBuzzImpl
    {
         private readonly string m_sGroupKey;

         public TvinciChannelFollowsBuzzImpl(int nGroupID, DateTime dtPeriod, TimeSpan dtInterval, List<string> lActions, List<string> lAssetTypes)
             : base(nGroupID, dtPeriod, dtInterval, lActions, lAssetTypes)
        {
            m_sGroupKey = string.Concat(m_nGroupID, "_channel_follows_stats");
        }

        protected override string GetGroupKey()
        {
            return m_sGroupKey;
        }
    }
}
