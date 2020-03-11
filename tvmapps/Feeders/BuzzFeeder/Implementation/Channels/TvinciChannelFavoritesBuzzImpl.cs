using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzFeeder.Implementation.Channels
{
    public class TvinciChannelFavoritesBuzzImpl : BaseChannelBuzzImpl
    {
         private readonly string m_sGroupKey;

         public TvinciChannelFavoritesBuzzImpl(int nGroupID, DateTime dtPeriod, TimeSpan dtInterval, int Weight, List<string> lActions, List<string> lAssetTypes, List<int> lFormulaWeights)
             : base(nGroupID, dtPeriod, dtInterval, Weight, lActions, lAssetTypes, lFormulaWeights)
        {
            m_sGroupKey = string.Concat(m_nGroupID, "_channel_favorites_stats");
        }

        protected override string GetGroupKey()
        {
            return m_sGroupKey;
        }
    }
}
