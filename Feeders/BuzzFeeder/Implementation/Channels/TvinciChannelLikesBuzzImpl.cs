using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzFeeder.Implementation.Channels
{
    public class TvinciChannelLikesBuzzImpl : BaseChannelBuzzImpl
    {
          private readonly string m_sGroupKey;
          public TvinciChannelLikesBuzzImpl(int nGroupID, DateTime dtPeriod, TimeSpan dtInterval, int Weight, List<string> lActions, List<string> lAssetTypes, List<int> lFormulaWeights)
              : base(nGroupID, dtPeriod, dtInterval, Weight, lActions, lAssetTypes, lFormulaWeights)
        {
            m_sGroupKey = string.Concat(m_nGroupID, "_channel_likes_stats");
        }

          protected override string GetGroupKey()
          {
              return m_sGroupKey;
          }
    }
}
