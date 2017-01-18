using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MediaPersonalStatistics : BaseCacheObject
    {
        public MediaPersonalStatistics()
        {
            m_nWatcherID = 0;
            m_nVoteVal = 0;
            m_bDidVote = false;
        }

        public override string GetCacheKey(int nMediaID)
        {
            string sKey = this.GetType().ToString() + "_" + m_nWatcherID.ToString() + "_" + nMediaID.ToString();
            return sKey;
        }

        public void Initialize(Int32 nWatcherID, Int32 nVoteVal, bool bDidVote)
        {
            m_nWatcherID = nWatcherID;
            m_nVoteVal = nVoteVal;
            m_bDidVote = bDidVote;
        }

        public Int32 m_nWatcherID;
        public Int32 m_nVoteVal;
        public bool m_bDidVote;
    }
}
