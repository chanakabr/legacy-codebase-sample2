using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzFeeder.BuzzCalculator
{
    [Serializable]
    public class ItemsStats
    {
        [JsonProperty("media_id")]
        public string sMediaID;
        [JsonProperty("periodical_growth")]
        public double nPeriodicalGrowth;
        [JsonProperty("delta_from_group_avg")]
        public double nDeltaFromGroupAverage;
        [JsonProperty("relative_periodical_growth")]
        public double nRelativePeriodicalGrowth;
        [JsonProperty("relative_cumulative_growth")]
        public double nRelativeCumulativeGrowth;
        [JsonProperty("sample_count")]
        public double nSampleCount;
        [JsonProperty("cumulative_count")]
        public double nSampleCumulativeCount;
        [JsonProperty("activity_measurement")]
        public double nActivityMeasurement;

        public ItemsStats()
        {
            nPeriodicalGrowth = 0.0;
            nRelativeCumulativeGrowth = 0.0;
            nRelativePeriodicalGrowth = 0.0;
            nDeltaFromGroupAverage = 0.0;
            nSampleCount = 0;
            nSampleCumulativeCount = 0;
            sMediaID = string.Empty;
            nActivityMeasurement = 0.0;
        }

        public static ItemsStats MergeItems(ItemsStats bucketA, ItemsStats bucketB)
        {
            ItemsStats newBucket = null;

            if (bucketA == null && bucketB == null)
                newBucket = new ItemsStats();
            else if (bucketA == null)
                newBucket = bucketB;
            else if (bucketB == null)
                newBucket = bucketA;
            else
            {
                newBucket = new ItemsStats()
                {
                    nSampleCount = bucketA.nSampleCount + bucketB.nSampleCount,
                    nSampleCumulativeCount = bucketA.nSampleCumulativeCount + bucketB.nSampleCumulativeCount,
                    sMediaID = bucketA.sMediaID
                };
            }

            return newBucket;
        }
    }

}
