using ApiObjects.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Catalog.Statistics
{
    public class LinearViews
    {
        private const int TIME_PERIOD = 30;  // In seconds
        private static readonly string INDEX = "statistics";
        private static readonly string TYPE = "stats";

        public int GetLiveViewsCount(int nChannelId)
        {
            int nViewsCount = 0;

            if (nChannelId != 0)
            {
                bool createdNew = false;
                var mutexSecurity = Utils.CreateMutex();

                using (Mutex mutex = new Mutex(false, string.Concat("Channel CID_", nChannelId), out createdNew, mutexSecurity))
                {
                    try
                    {
                        //_logger.Info(string.Format("{0} : {1}", "Lock", string.Concat("Group GID_", nChannelId)));
                        mutex.WaitOne(-1);
                        nViewsCount = BucketsCache.Instance.GetLiveViewsCount(nChannelId);
                    }
                    catch
                    {

                    }
                    finally
                    {
                        mutex.ReleaseMutex();
                    }
                }

            }

            return nViewsCount;
        }


        public bool UpdateOrWriteBuckets(DateTime updateTime, int nChannelId, int nGroupId, string sMediaTypeId)
        {
            bool bIsUpdatedOrInserted = false;

            if (nChannelId != 0 && nGroupId != 0)
            {
                string sMediaTypeFromConfig = Utils.GetWSURL(string.Format("LinearTypeId_{0}", nGroupId));

                // Only if it is a linear channle
                if (!string.IsNullOrEmpty(sMediaTypeFromConfig) && sMediaTypeFromConfig.Equals(sMediaTypeId))
                {
                    bool createdNew = false;
                    var mutexSecurity = Utils.CreateMutex();

                    using (Mutex mutex = new Mutex(false, string.Concat("Channel CID_", nChannelId), out createdNew, mutexSecurity))
                    {
                        try
                        {
                            //_logger.Info(string.Format("{0} : {1}", "Lock", string.Concat("Group GID_", nChannelId)));
                            mutex.WaitOne(-1);
                            ChannelBucket bucket = BucketsCache.Instance.GetBucketByChannelId(nChannelId);

                            if (bucket != null) // Linear channel is already in cache
                            {
                                DateTime nextThirtySecondsDate = bucket.CurrentDate.AddSeconds(TIME_PERIOD);
                                if (updateTime < nextThirtySecondsDate)
                                {
                                    bucket.CurrentViews++;
                                }
                                else
                                {
                                    #region Write To DB/ES

                                    Tvinci.Core.DAL.CatalogDAL.InsertOrUpdate_ChannelsLiveViews(bucket, nChannelId, nGroupId);
                                    bool bSuccesss = WriteLiveViewsToES(nGroupId, nChannelId, sMediaTypeId, bucket);
                                    #endregion

                                    #region Copy current values to previous and update date

                                    bucket.PreviousViews = bucket.CurrentViews;
                                    bucket.CurrentViews = 1;
                                    bucket.CurrentDate = DateTime.UtcNow;

                                    #endregion

                                }

                                bIsUpdatedOrInserted = true;

                            }
                            else
                            {
                                bucket = new ChannelBucket();
                                bucket.CurrentViews++;
                                bIsUpdatedOrInserted = BucketsCache.Instance.InsertChannelBucket(nChannelId, bucket);
                            }
                        }
                        catch
                        {
                            // Write Log
                        }
                        finally
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }
            }

            return bIsUpdatedOrInserted;
        }

        private bool WriteLiveViewsToES(int nGroupId, int nChannelId, string sMediaTypeID, ChannelBucket bucket)
        {
            bool bRes = false;
            ElasticSearch.Common.ElasticSearchApi oESApi = new ElasticSearch.Common.ElasticSearchApi();

            MediaView view = new MediaView() { GroupID = nGroupId, MediaType = sMediaTypeID, MediaID = nChannelId };

            string sJsonView = Newtonsoft.Json.JsonConvert.SerializeObject(view);

            if (oESApi.IndexExists(INDEX) && !string.IsNullOrEmpty(sJsonView))
            {
                bRes = oESApi.InsertRecord(INDEX, TYPE, nChannelId.ToString(), sJsonView);

                if(!bRes)
                    Logger.Logger.Log("Error", string.Format("Was unable to insert record to ES. index={0};type={1};doc={2}", INDEX, TYPE, sJsonView), "CatalogStats");
            }

            return bRes;
        }
    }

}
