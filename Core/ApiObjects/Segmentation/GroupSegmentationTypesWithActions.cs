using System.Collections.Generic;
using CachingProvider.LayeredCache;
using CouchbaseManager;

namespace ApiObjects.Segmentation
{
    public class GroupSegmentationTypesWithActions
    {
        public static string GetKey(int groupId)
        {
            return $"GroupSegmentationTypesWithActions_{groupId}";
        }

        private static bool Set(int groupId, HashSet<long> segmentationTypeIds)
        {
            string key = GetKey(groupId);

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            bool setResult = couchbaseManager.Set<HashSet<long>>(key, segmentationTypeIds);
            return setResult;
        }

        public static HashSet<long> Get(int groupId)
        {
            string key = GetKey(groupId);

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            return couchbaseManager.Get<HashSet<long>>(key);
        }

        public static bool Update(int groupId, long segmentTypeId, bool add = true)
        {
            string key = GetKey(groupId);

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            var segmentationTypeIds = couchbaseManager.Get<HashSet<long>>(key);

            if (segmentationTypeIds != null)
            {
                bool exist = segmentationTypeIds.Contains(segmentTypeId);
                if (add && !exist)
                {
                    segmentationTypeIds.Add(segmentTypeId);
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupSegmentationTypeIdsOfActionInvalidationKey(groupId));
                    return Set(groupId, segmentationTypeIds);
                }
                if (!add && exist)
                {
                    segmentationTypeIds.Remove(segmentTypeId);
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupSegmentationTypeIdsOfActionInvalidationKey(groupId));
                    return Set(groupId, segmentationTypeIds);
                }
            }
            else
            {
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupSegmentationTypeIdsOfActionInvalidationKey(groupId));
                return Init(groupId);
            }

            return true;
        }

        public static bool Init(int groupId)
        {
            HashSet<long> ids = new HashSet<long>();

            var segments = SegmentationType.ListFromCb(groupId, null, 0, 0, out int totalCount);
            if (totalCount > 0)
            {
                foreach (var segment in segments)
                {
                    if (segment.Actions?.Count > 0)
                    {
                        ids.Add(segment.Id);
                    }
                }
            }

            return Set(groupId, ids);
        }
    }
}