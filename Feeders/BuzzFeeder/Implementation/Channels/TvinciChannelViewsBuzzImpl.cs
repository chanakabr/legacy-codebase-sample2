using BuzzFeeder.BuzzCalculator;
using ElasticSearch.Searcher;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace BuzzFeeder.Implementation.Channels
{
    public class TvinciChannelViewsBuzzImpl : BaseChannelBuzzImpl
    {
        
        protected readonly string m_sGroupKey;
        public TvinciChannelViewsBuzzImpl(int nGroupID, DateTime dtPeriod, TimeSpan dtInterval, List<string> lActions, List<string> lMediaTypes)
            : base(nGroupID, dtPeriod, dtInterval, lActions, lMediaTypes)
        {
            m_sGroupKey = string.Concat(m_nGroupID, "_channel_views_stats");
        }

        protected override string GetGroupKey()
        {
            return m_sGroupKey;
        }
   
    }
}
