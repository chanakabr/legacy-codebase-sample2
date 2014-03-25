using ApiObjects.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Catalog.Statistics
{
    public class BucketsCache
    {
        #region Members

        public ConcurrentDictionary<int, ChannelBucket> BucketsData { get; set; }
        private const int TIME_PERIOD = 30;  // In seconds


        #endregion

        #region CTOR

        private BucketsCache()
        {
            BucketsData = new ConcurrentDictionary<int, ChannelBucket>();
        }

        #endregion

        #region Singleton

        public static BucketsCache Instance
        {
            get { return Nested.Instance; }
        }

        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly BucketsCache Instance = new BucketsCache();
        }

        #endregion

        #region Public

        public int GetLiveViewsCount(int nChannelId)
        {
            int nLiveViewsCount = 0;

            if (BucketsData.ContainsKey(nChannelId))
            {
                ChannelBucket bucket = null;
                bool isBucketFound = BucketsData.TryGetValue(nChannelId, out bucket);
                if (bucket != null)
                {
                    nLiveViewsCount = bucket.CurrentViews;
                }
            }

            return nLiveViewsCount;
        }

        public bool InsertChannelBucket(int nChannelId, ChannelBucket bucket)
        {
            bool isBucketInserted = false;

            if (!BucketsData.ContainsKey(nChannelId))
            {
                isBucketInserted = BucketsData.TryAdd(nChannelId, bucket);
            }
            else
            {
                isBucketInserted = false; // Bucket exists
            }

            return isBucketInserted;
        }

        public ChannelBucket GetBucketByChannelId(int nChannelId)
        {
            ChannelBucket bucket = null;

            if (BucketsData.ContainsKey(nChannelId))
            {
                bool isBucketFound = BucketsData.TryGetValue(nChannelId, out bucket);
            }

            return bucket;
        }

        public void UpdateBucket(int nChannelId, ChannelBucket bucket)
        {
            if (BucketsData.ContainsKey(nChannelId))
            {
                BucketsData[nChannelId] = bucket;
            }
        }

        #endregion

    }

}
