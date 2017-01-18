using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MediaStatistics : BaseCacheObject
    {
        public MediaStatistics()
        {
            m_nTotalViews = 0;
            m_nRatingSum = 0;
            m_nRatingCount = 0;
            m_nLoRateCount = 0;
            m_nHiRateCount = 0;
            m_n1RateCount = 0;
            m_n2RateCount = 0;
            m_n3RateCount = 0;
            m_n4RateCount = 0;
            m_n5RateCount = 0;
        }

        public override string GetCacheKey(Int32 nMediaID)
        {
            string sKey = this.GetType().ToString() + "_" + nMediaID.ToString();
            return sKey;
        }

        public void Initialize(Int32 nTotalViews, Int32 nRatingSum, Int32 nRatingCount, Int32 nLoRateCount,
            Int32 nHiRateCount , Int32 n1RateCount, Int32 n2RateCount, Int32 n3RateCount, Int32 n4RateCount,
            Int32 n5RateCount)
        {
            m_nTotalViews = nTotalViews;
            m_nRatingSum = nRatingSum;
            m_nRatingCount = nRatingCount;
            m_nLoRateCount = nLoRateCount;
            m_nHiRateCount = nHiRateCount;
            m_n1RateCount = n1RateCount;
            m_n2RateCount = n2RateCount;
            m_n3RateCount = n3RateCount;
            m_n4RateCount = n4RateCount;
            m_n5RateCount = n5RateCount;
        }

        public Int32 m_nTotalViews;
        public Int32 m_nRatingSum;
        public Int32 m_nRatingCount;
        public Int32 m_nLoRateCount;
        public Int32 m_nHiRateCount;
        public Int32 m_n1RateCount;
        public Int32 m_n2RateCount;
        public Int32 m_n3RateCount;
        public Int32 m_n4RateCount;
        public Int32 m_n5RateCount;
    }
}
